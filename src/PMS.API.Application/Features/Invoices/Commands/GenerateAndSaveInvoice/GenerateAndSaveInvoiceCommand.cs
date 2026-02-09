using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Constants;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Invoices.DTO;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;
using PMS.API.Infrastructure.Interfaces;

namespace PMS.API.Application.Features.Invoices.Commands.GenerateAndSaveInvoice;

/// <summary>
/// Source of the generate request: Organization tab or Patient tab.
/// </summary>
public static class InvoiceGenerationType
{
  public const string Organization = "Organization";
  public const string Patient = "Patient";
}

/// <summary>
/// Creates invoice history entries (or replaces unsent same-period ones), generates files, marks Completed.
/// Skips orgs/patients that already have a sent invoice for the period and returns their names to the UI.
/// </summary>
public class GenerateAndSaveInvoiceCommand : IRequest<ApplicationResult<GenerateAndSaveInvoiceResultDto>>
{
  /// <summary>Source of the request: "Organization" or "Patient". Determines which IDs are used and whether to process orgs or patients.</summary>
  [Required]
  public string GenerationType { get; set; } = string.Empty;

  /// <summary>When GenerationType is Organization: external org IDs, or null/empty for all orgs without an invoice for the period.</summary>
  public List<long>? OrganizationIds { get; set; }

  /// <summary>When GenerationType is Patient: internal patient IDs, or null/empty for all patients without an invoice for the period.</summary>
  public List<long>? PatientIds { get; set; }

  [Required]
  public DateTime FromDate { get; set; }

  [Required]
  public DateTime ToDate { get; set; }

  public bool IsSent { get; set; }
}

public class GenerateAndSaveInvoiceCommandHandler : RequestHandlerBase<GenerateAndSaveInvoiceCommand, ApplicationResult<GenerateAndSaveInvoiceResultDto>>
{
  private readonly AppDbContext _appDbContext;
  private readonly ICurrentUserService _currentUserService;

  public GenerateAndSaveInvoiceCommandHandler(
    IServiceProvider serviceProvider,
    ILogger<GenerateAndSaveInvoiceCommandHandler> logger,
    AppDbContext appDbContext,
    ICurrentUserService currentUserService)
    : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
    _currentUserService = currentUserService;
  }

  protected override async Task<ApplicationResult<GenerateAndSaveInvoiceResultDto>> HandleRequest(
    GenerateAndSaveInvoiceCommand request,
    CancellationToken cancellationToken)
  {
    if (request.FromDate > request.ToDate)
      return ApplicationResult<GenerateAndSaveInvoiceResultDto>.Error("FromDate cannot be greater than ToDate.");

    var type = request.GenerationType?.Trim();
    if (string.IsNullOrEmpty(type) || (type != InvoiceGenerationType.Organization && type != InvoiceGenerationType.Patient))
      return ApplicationResult<GenerateAndSaveInvoiceResultDto>.Error("GenerationType must be \"Organization\" or \"Patient\".");

    var skippedOrgs = new List<string>();
    var skippedPatients = new List<string>();

    if (type == InvoiceGenerationType.Organization)
    {
      var orgIdsToProcess = request.OrganizationIds != null && request.OrganizationIds.Count > 0
        ? request.OrganizationIds
        : await GetOrganizationIdsWithoutInvoiceForPeriodAsync(request.FromDate, request.ToDate, cancellationToken);

      foreach (var organizationExternalId in orgIdsToProcess)
      {
        var (skipped, created) = await ProcessOrganizationInvoice(
          organizationExternalId, request, skippedOrgs, cancellationToken);
        if (skipped)
          continue;
        if (!created)
          continue;
      }
    }
    else
    {
      var patientIdsToProcess = request.PatientIds != null && request.PatientIds.Count > 0
        ? request.PatientIds
        : await GetPatientIdsWithoutInvoiceForPeriodAsync(request.FromDate, request.ToDate, cancellationToken);

      foreach (var patientId in patientIdsToProcess)
      {
        var (skipped, created) = await ProcessPatientInvoice(
          patientId, request, skippedPatients, cancellationToken);
        if (skipped)
          continue;
        if (!created)
          continue;
      }
    }

    await _appDbContext.SaveChangesAsync(cancellationToken);

    var invoiceProcessingService = _serviceProvider.GetRequiredService<IInvoiceProcessingService>();
    await invoiceProcessingService.ProcessPendingInvoicesAsync(cancellationToken);

    var result = new GenerateAndSaveInvoiceResultDto
    {
      SkippedOrganizationNames = skippedOrgs,
      SkippedPatientNames = skippedPatients
    };
    return ApplicationResult<GenerateAndSaveInvoiceResultDto>.SuccessResult(result);
  }

  /// <summary>Organization external IDs (OrganizationExternalId) that have no invoice overlapping the period.</summary>
  private async Task<List<long>> GetOrganizationIdsWithoutInvoiceForPeriodAsync(
    DateTime fromDate, DateTime toDate, CancellationToken ct)
  {
    var orgIdsWithInvoice = await _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => !h.IsDeleted && h.OrganizationId != null &&
        h.InvoiceStartDate <= toDate && h.InvoiceEndDate >= fromDate)
      .Select(h => h.OrganizationId!.Value)
      .Distinct()
      .ToListAsync(ct);

    if (orgIdsWithInvoice.Count == 0)
      return await _appDbContext.Organization
        .AsNoTracking()
        .Where(o => !o.IsDeleted)
        .Select(o => o.OrganizationExternalId)
        .ToListAsync(ct);

    return await _appDbContext.Organization
      .AsNoTracking()
      .Where(o => !o.IsDeleted && !orgIdsWithInvoice.Contains(o.Id))
      .Select(o => o.OrganizationExternalId)
      .ToListAsync(ct);
  }

  /// <summary>Patient internal IDs (Patient.Id) that have no invoice overlapping the period.</summary>
  private async Task<List<long>> GetPatientIdsWithoutInvoiceForPeriodAsync(
    DateTime fromDate, DateTime toDate, CancellationToken ct)
  {
    var patientIdsWithInvoice = await _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => !h.IsDeleted && h.PatientId != null &&
        h.InvoiceStartDate <= toDate && h.InvoiceEndDate >= fromDate)
      .Select(h => h.PatientId!.Value)
      .Distinct()
      .ToListAsync(ct);

    if (patientIdsWithInvoice.Count == 0)
      return await _appDbContext.Patient
        .AsNoTracking()
        .Where(p => !p.IsDeleted)
        .Select(p => p.Id)
        .ToListAsync(ct);

    return await _appDbContext.Patient
      .AsNoTracking()
      .Where(p => !p.IsDeleted && !patientIdsWithInvoice.Contains(p.Id))
      .Select(p => p.Id)
      .ToListAsync(ct);
  }

  /// <returns>(skipped because already sent, created new or replaced)</returns>
  private async Task<(bool skipped, bool created)> ProcessOrganizationInvoice(
    long organizationExternalId,
    GenerateAndSaveInvoiceCommand request,
    List<string> skippedOrganizationNames,
    CancellationToken ct)
  {
    var org = await _appDbContext.Organization
      .AsNoTracking()
      .Where(x => x.OrganizationExternalId == organizationExternalId && !x.IsDeleted)
      .Select(x => new { x.Id, x.Name })
      .FirstOrDefaultAsync(ct);

    if (org == null)
      return (false, false);

    var existing = await _appDbContext.InvoiceHistory
      .Where(h =>
        !h.IsDeleted &&
        h.OrganizationId == org.Id &&
        h.InvoiceStartDate <= request.ToDate &&
        h.InvoiceEndDate >= request.FromDate)
      .Include(h => h.InvoiceHistoryWardList)
      .ToListAsync(ct);

    if (existing.Any())
    {
      var anySent = existing.Any(h => h.IsSent);
      if (anySent)
      {
        skippedOrganizationNames.Add(org.Name);
        return (true, false);
      }
      // Same period, not sent: reuse one record (regenerate in place), soft-delete the rest
      var toKeep = existing.OrderByDescending(h => h.Id).First();
      foreach (var h in existing.Where(h => h.Id != toKeep.Id))
      {
        h.IsDeleted = true;
        h.ModifiedDate = DateTime.UtcNow;
      }
      var wardsWithPatients = await _appDbContext.Ward
        .Where(w => w.OrganizationId == org.Id)
        .Select(w => new { WardId = w.Id, PatientIds = new List<long>() })
        .ToListAsync(ct);
      toKeep.FilePath = null;
      toKeep.InvoiceStartDate = request.FromDate;
      toKeep.InvoiceEndDate = request.ToDate;
      toKeep.IsSent = request.IsSent;
      toKeep.ModifiedDate = DateTime.UtcNow;
      InvoiceStatusHistoryHelper.AppendStatus(toKeep, InvoiceStatusConstants.Pending);
      toKeep.InvoiceHistoryWardList.Clear();
      foreach (var w in wardsWithPatients)
        toKeep.InvoiceHistoryWardList.Add(new InvoiceHistoryWard { InvoiceHistoryId = toKeep.Id, WardId = w.WardId, PatientIds = "" });
      return (false, true);
    }

    var wardsWithPatientsNew = await _appDbContext.Ward
      .Where(w => w.OrganizationId == org.Id)
      .Select(w => new
      {
        WardId = w.Id,
        PatientIds = new List<long>()
      })
      .ToListAsync(ct);

    var history = new InvoiceHistory
    {
      OrganizationId = org.Id,
      PatientId = null,
      InvoiceStartDate = request.FromDate,
      InvoiceEndDate = request.ToDate,
      InvoiceStatus = InvoiceStatusConstants.Pending,
      InvoiceStatusHistory = null,
      FilePath = null,
      IsSent = request.IsSent,
      CreatedBy = _currentUserService.UserId,
      CreatedDate = DateTime.UtcNow,
      InvoiceHistoryWardList = wardsWithPatientsNew
        .Select(w => new InvoiceHistoryWard
        {
          WardId = w.WardId,
          PatientIds = string.Join(",", w.PatientIds)
        })
        .ToList()
    };
    InvoiceStatusHistoryHelper.AppendStatus(history, InvoiceStatusConstants.Pending);
    _appDbContext.InvoiceHistory.Add(history);
    return (false, true);
  }

  /// <returns>(skipped because already sent, created new or replaced)</returns>
  private async Task<(bool skipped, bool created)> ProcessPatientInvoice(
    long patientId,
    GenerateAndSaveInvoiceCommand request,
    List<string> skippedPatientNames,
    CancellationToken ct)
  {
    var patient = await _appDbContext.Patient
      .AsNoTracking()
      .Where(p => p.Id == patientId && !p.IsDeleted)
      .Select(p => new { p.Id, p.Name })
      .FirstOrDefaultAsync(ct);

    if (patient == null)
      return (false, false);

    var existing = await _appDbContext.InvoiceHistory
      .Where(h =>
        !h.IsDeleted &&
        h.PatientId == patientId &&
        h.InvoiceStartDate <= request.ToDate &&
        h.InvoiceEndDate >= request.FromDate)
      .ToListAsync(ct);

    if (existing.Any())
    {
      var anySent = existing.Any(h => h.IsSent);
      if (anySent)
      {
        skippedPatientNames.Add(patient.Name);
        return (true, false);
      }
      // Same period, not sent: reuse one record (regenerate in place), soft-delete the rest
      var toKeep = existing.OrderByDescending(h => h.Id).First();
      foreach (var h in existing.Where(h => h.Id != toKeep.Id))
      {
        h.IsDeleted = true;
        h.ModifiedDate = DateTime.UtcNow;
      }
      toKeep.FilePath = null;
      toKeep.InvoiceStartDate = request.FromDate;
      toKeep.InvoiceEndDate = request.ToDate;
      toKeep.IsSent = request.IsSent;
      toKeep.ModifiedDate = DateTime.UtcNow;
      InvoiceStatusHistoryHelper.AppendStatus(toKeep, InvoiceStatusConstants.Pending);
      return (false, true);
    }

    var history = new InvoiceHistory
    {
      OrganizationId = null,
      PatientId = patientId,
      InvoiceStartDate = request.FromDate,
      InvoiceEndDate = request.ToDate,
      InvoiceStatus = InvoiceStatusConstants.Pending,
      InvoiceStatusHistory = null,
      FilePath = null,
      IsSent = request.IsSent,
      CreatedBy = _currentUserService.UserId,
      CreatedDate = DateTime.UtcNow
    };
    InvoiceStatusHistoryHelper.AppendStatus(history, InvoiceStatusConstants.Pending);
    _appDbContext.InvoiceHistory.Add(history);
    return (false, true);
  }
}
