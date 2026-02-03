using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common.Constants;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Services.Implementation;

public class InvoiceProcessingService : IInvoiceProcessingService
{
  private readonly AppDbContext _appDbContext;
  private readonly IInvoiceFileGenerationService _invoiceFileGenerationService;
  private readonly ILogger<InvoiceProcessingService> _logger;

  public InvoiceProcessingService(
    AppDbContext appDbContext,
    IInvoiceFileGenerationService invoiceFileGenerationService,
    ILogger<InvoiceProcessingService> logger)
  {
    _appDbContext = appDbContext;
    _invoiceFileGenerationService = invoiceFileGenerationService;
    _logger = logger;
  }

  public async Task ProcessPendingInvoicesAsync(CancellationToken cancellationToken = default)
  {
    var pending = await _appDbContext.InvoiceHistory
      .Where(h => h.InvoiceStatus == InvoiceStatusConstants.Pending && !h.IsDeleted)
      .Include(h => h.InvoiceHistoryWardList)
      .OrderBy(h => h.CreatedDate)
      .ToListAsync(cancellationToken);

    if (pending.Count == 0)
      return;

    _logger.LogInformation("Processing {Count} pending invoice(s).", pending.Count);

    foreach (var record in pending)
    {
      try
      {
        if (record.OrganizationId.HasValue && record.OrganizationId.Value > 0)
          await ProcessOrganizationInvoiceAsync(record, cancellationToken);
        else if (record.PatientId.HasValue && record.PatientId.Value > 0)
          await ProcessPatientInvoiceAsync(record, cancellationToken);
        else
        {
          _logger.LogWarning("Invoice history Id {Id} has neither OrganizationId nor PatientId; marking as Failed.", record.Id);
          InvoiceStatusHistoryHelper.AppendStatus(record, InvoiceStatusConstants.Failed);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to process pending invoice Id {Id}. Marking as Failed.", record.Id);
        InvoiceStatusHistoryHelper.AppendStatus(record, InvoiceStatusConstants.Failed);
      }
    }

    await _appDbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task ProcessOrganizationInvoiceAsync(InvoiceHistory record, CancellationToken ct)
  {
    var (filePath, charges) = await _invoiceFileGenerationService.GenerateOrganizationInvoiceAsync(
      record.OrganizationId!.Value,
      record.InvoiceStartDate,
      record.InvoiceEndDate,
      ct);

    if (string.IsNullOrEmpty(filePath) || charges == null || charges.Count == 0)
    {
      InvoiceStatusHistoryHelper.AppendStatus(record, InvoiceStatusConstants.Failed);
      return;
    }

    record.FilePath = filePath;
    InvoiceStatusHistoryHelper.AppendStatus(record, InvoiceStatusConstants.Completed);
    record.ModifiedDate = DateTime.UtcNow;

    record.InvoiceHistoryWardList?.Clear();
    foreach (var g in charges.GroupBy(x => x.WardId))
    {
      record.InvoiceHistoryWardList ??= new List<InvoiceHistoryWard>();
      record.InvoiceHistoryWardList.Add(new InvoiceHistoryWard
      {
        InvoiceHistoryId = record.Id,
        WardId = g.Key,
        PatientIds = string.Join(",", g.Select(x => x.PatientId).Distinct())
      });
    }
  }

  private async Task ProcessPatientInvoiceAsync(InvoiceHistory record, CancellationToken ct)
  {
    var filePath = await _invoiceFileGenerationService.GeneratePatientInvoiceAsync(
      record.PatientId!.Value,
      record.InvoiceStartDate,
      record.InvoiceEndDate,
      ct);

    if (string.IsNullOrEmpty(filePath))
    {
      InvoiceStatusHistoryHelper.AppendStatus(record, InvoiceStatusConstants.Failed);
      return;
    }

    record.FilePath = filePath;
    InvoiceStatusHistoryHelper.AppendStatus(record, InvoiceStatusConstants.Completed);
    record.ModifiedDate = DateTime.UtcNow;
    await Task.CompletedTask;
  }
}
