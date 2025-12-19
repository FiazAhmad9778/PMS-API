using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces.Repositories;

namespace PMS.API.Application.Services.Implementation;

public class OrderFaxService : IOrderFaxService
{
  private readonly IConfiguration _configuration;
  private readonly IOrderRepository _orderRepository;
  private readonly IInterFaxService _interFaxService;
  private readonly IPdfService _pdfService;
  private readonly ILogger<OrderFaxService> _logger;

  public OrderFaxService(
    IConfiguration configuration,
    IOrderRepository orderRepository,
    IInterFaxService interFaxService,
    IPdfService pdfService,
    ILogger<OrderFaxService> logger)
  {
    _configuration = configuration;
    _orderRepository = orderRepository;
    _interFaxService = interFaxService;
    _pdfService = pdfService;
    _logger = logger;
  }

  public async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("Processing all pending orders");

      // Get all pending orders
      var pendingOrders = _orderRepository.Get()
        .Where(o => o.FaxStatus == "Pending" || o.FaxStatus == "Retrying")
        .Where(o => o.FaxRetryCount < 3)
        .ToList();

      _logger.LogInformation("Found {Count} pending orders", pendingOrders.Count);

      var faxNumber = _configuration["InterFax:DefaultFaxNumber"];
      if (string.IsNullOrEmpty(faxNumber))
      {
        _logger.LogError("Fax number not configured");
        return;
      }

      foreach (var order in pendingOrders)
      {
        try
        {
          await ProcessOrderFaxAsync(order, faxNumber, cancellationToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error processing fax for OrderId: {OrderId}", order.Id);
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error processing pending orders");
    }
  }

  private async Task ProcessOrderFaxAsync(Order order, string faxNumber, CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Processing fax for OrderId: {OrderId}", order.Id);

      // Skip if already sent successfully
      if (order.FaxStatus == "Sent")
      {
        _logger.LogInformation("Order {OrderId} already sent successfully", order.Id);
        return;
      }

      // Generate PDF from order data
      _logger.LogInformation("Generating PDF for OrderId: {OrderId}", order.Id);
      var pdfBytes = await _pdfService.GenerateOrderPdfAsync(order);

      // Send fax via InterFAX
      var (success, transactionId, errorMessage) = await _interFaxService.SendFaxAsync(
        faxNumber,
        pdfBytes,
        $"order_{order.Id}.pdf");

      // Update order status
      if (success)
      {
        order.FaxStatus = "Sent";
        order.FaxTransactionId = transactionId;
        order.FaxSentAt = DateTime.UtcNow;
        order.FaxRetryCount++;
        order.FaxErrorMessage = null;
        _logger.LogInformation("Fax sent successfully for OrderId: {OrderId}, TransactionId: {TransactionId}",
          order.Id, transactionId);
      }
      else
      {
        order.FaxStatus = "Failed";
        order.FaxErrorMessage = errorMessage;
        order.FaxRetryCount++;
        _logger.LogWarning("Fax failed for OrderId: {OrderId}, Error: {Error}", order.Id, errorMessage);
      }

      await _orderRepository.UpdateAsync(order);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error processing fax for OrderId: {OrderId}", order.Id);

      // Update order status to failed
      try
      {
        order.FaxStatus = "Failed";
        order.FaxErrorMessage = ex.Message;
        order.FaxRetryCount++;
        await _orderRepository.UpdateAsync(order);
      }
      catch (Exception updateEx)
      {
        _logger.LogError(updateEx, "Error updating order status for OrderId: {OrderId}", order.Id);
      }
    }
  }
}
