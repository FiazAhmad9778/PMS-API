using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Orders.Commands.CreateOrderFromWebhook;
using PMS.API.Application.Features.Orders.DTOs;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
[ApiController]
public class TestController : BaseApiController
{
  public TestController(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  [HttpPost("send-test-fax")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<long>> SendTestFax([FromBody] TestOrderRequest request)
  {
    try
    {
      // Create mock webflow data using OR-* fields + dynamic medications
      var meds = request.Medications ?? new List<string> { "Test Medication - Paracetamol 500mg" };
      var primaryMed = meds.FirstOrDefault() ?? "Test Medication - Paracetamol 500mg";

      var additionalFields = new Dictionary<string, JsonElement>();
      var extraMeds = meds.Skip(1).ToList();
      for (int i = 0; i < extraMeds.Count; i++)
      {
        var key = $"Medication {i + 2}";
        additionalFields[key] = JsonSerializer.SerializeToElement(extraMeds[i]);
      }

      var webflowData = new WebflowWebhookDto
      {
        OrName = request.FirstName ?? "John",
        OrLastName = request.LastName ?? "Doe",
        OrPhoneNumber = request.PhoneNumber ?? "1234567890",
        OrMedication = primaryMed,
        OrDeliveryOrPickUp = request.DeliveryOrPickup ?? "Delivery",
        OrTomorrowDeliveryTime = request.DeliveryTimeSlot ?? "10AM",
        OrNote = request.Notes ?? "This is a test order for fax functionality",
        AdditionalFields = additionalFields
      };

      // Create order using the same command handler
      var command = new CreateOrderFromWebhookCommand
      {
        WebflowData = webflowData
      };

      var result = await Mediator.Send(command);

      if (result.IsSuccess && result.Data > 0)
      {
        // Optionally trigger immediate processing (or wait for recurring job)
        if (request.ProcessImmediately)
        {
          var orderFaxService = HttpContext.RequestServices.GetRequiredService<IOrderFaxService>();
          _ = Task.Run(async () =>
          {
            await Task.Delay(1000); // Small delay to ensure order is saved
            await orderFaxService.ProcessPendingOrdersAsync();
          });
        }

        return ApplicationResult<long>.SuccessResult(
          result.Data,
          $"Test order created successfully. Order ID: {result.Data}. " +
          (request.ProcessImmediately 
            ? "Processing fax immediately..." 
            : "Fax will be processed by recurring job (every 10 minutes)."));
      }

      return result;
    }
    catch (Exception ex)
    {
      return ApplicationResult<long>.Error($"Error creating test order: {ex.Message}");
    }
  }
}

public class TestOrderRequest
{
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public string? PhoneNumber { get; set; }
  public List<string>? Medications { get; set; }
  public string? DeliveryOrPickup { get; set; }
  public string? Address { get; set; }
  public string? DeliveryTimeSlot { get; set; }
  public string? Notes { get; set; }
  public bool ProcessImmediately { get; set; } = false;
}

