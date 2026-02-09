using System.IO;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Constants;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Invoices.Commands.ResendInvoicesEmail;

/// <summary>
/// Resends the invoice email for a single org or patient. Uses the latest generated invoice (with file).
/// After successful send, marks the invoice as sent (IsSent = true) so the UI shows correct status when it uses this API for both send and resend.
/// Provide exactly one of OrganizationId or PatientId.
/// </summary>
public class ResendInvoicesEmailCommand : IRequest<ApplicationResult<bool>>
{
  /// <summary>Internal organization ID (Organization.Id). Use when resending for an org.</summary>
  public long? OrganizationId { get; set; }
  /// <summary>Internal patient ID (Patient.Id). Use when resending for a patient.</summary>
  public long? PatientId { get; set; }
  /// <summary>Web root path (e.g. from IWebHostEnvironment.WebRootPath) to resolve invoice file paths.</summary>
  public string? WebRootPath { get; set; }
}

public class ResendInvoicesEmailCommandHandler : RequestHandlerBase<ResendInvoicesEmailCommand, ApplicationResult<bool>>
{
  private readonly AppDbContext _appDbContext;
  private readonly IEmailSenderService _emailSenderService;

  public ResendInvoicesEmailCommandHandler(
    IServiceProvider serviceProvider,
    ILogger<ResendInvoicesEmailCommandHandler> logger,
    AppDbContext appDbContext,
    IEmailSenderService emailSenderService)
    : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
    _emailSenderService = emailSenderService;
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(
    ResendInvoicesEmailCommand request,
    CancellationToken cancellationToken)
  {
    var hasOrg = request.OrganizationId.HasValue && request.OrganizationId.Value > 0;
    var hasPatient = request.PatientId.HasValue && request.PatientId.Value > 0;
    if (hasOrg == hasPatient)
      return ApplicationResult<bool>.Error("Provide exactly one of OrganizationId or PatientId.");

    var basePath = string.IsNullOrEmpty(request.WebRootPath) ? "wwwroot" : request.WebRootPath;

    if (hasOrg)
    {
      var orgId = request.OrganizationId!.Value;
      // Latest by last created (CreatedDate), not by invoice period from/to (tracked so we can update IsSent)
      var latest = await _appDbContext.InvoiceHistory
        .Where(h => !h.IsDeleted && h.OrganizationId == orgId && h.FilePath != null && h.FilePath != "")
        .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
        .FirstOrDefaultAsync(cancellationToken);
      if (latest == null)
        return ApplicationResult<bool>.Error("No invoice found for this organization.");

      var org = await _appDbContext.Organization
        .AsNoTracking()
        .Where(o => o.Id == orgId)
        .Select(o => new { o.Name, o.DefaultEmail })
        .FirstOrDefaultAsync(cancellationToken);
      if (org == null)
        return ApplicationResult<bool>.Error("Organization not found.");

      var toEmail = org.DefaultEmail?.Trim();
      if (string.IsNullOrEmpty(toEmail))
        return ApplicationResult<bool>.Error("Organization has no default email.");

      var fullPath = Path.Combine(basePath, latest.FilePath!);
      if (!File.Exists(fullPath))
        return ApplicationResult<bool>.Error("Invoice file not found.");

      var fromTo = $"{latest.InvoiceStartDate:MMM d, yyyy} to {latest.InvoiceEndDate:MMM d, yyyy}";
      var subject = $"{org.Name} - {fromTo} Invoice";
      var body = $"Hello {org.Name},\n\nPlease find your invoice attached ({fromTo}).\n\nBest regards,\nThe PMS Team";
      var sent = await _emailSenderService.SendEmailWithAttachment(toEmail, org.Name ?? "Customer", subject, body, fullPath, Path.GetFileName(latest.FilePath));
      if (sent)
      {
        latest.IsSent = true;
        latest.ModifiedDate = DateTime.UtcNow;
        InvoiceStatusHistoryHelper.AppendStatus(latest, InvoiceStatusConstants.Sent);
        _appDbContext.InvoiceHistory.Update(latest);
        await _appDbContext.SaveChangesAsync(cancellationToken);
      }
      return sent ? ApplicationResult<bool>.SuccessResult(true) : ApplicationResult<bool>.Error("Failed to send email.");
    }

    var patientId = request.PatientId!.Value;
    // Latest by last created (CreatedDate), not by invoice period from/to (tracked so we can update IsSent)
    var latestPatient = await _appDbContext.InvoiceHistory
      .Where(h => !h.IsDeleted && h.PatientId == patientId && h.FilePath != null && h.FilePath != "")
      .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
      .FirstOrDefaultAsync(cancellationToken);
    if (latestPatient == null)
      return ApplicationResult<bool>.Error("No invoice found for this patient.");

    var patient = await _appDbContext.Patient
      .AsNoTracking()
      .Where(p => p.Id == patientId)
      .Select(p => new { p.Name, p.DefaultEmail })
      .FirstOrDefaultAsync(cancellationToken);
    if (patient == null)
      return ApplicationResult<bool>.Error("Patient not found.");

    var patientToEmail = patient.DefaultEmail?.Trim();
    if (string.IsNullOrEmpty(patientToEmail))
      return ApplicationResult<bool>.Error("Patient has no default email.");

    var patientFullPath = Path.Combine(basePath, latestPatient.FilePath!);
    if (!File.Exists(patientFullPath))
      return ApplicationResult<bool>.Error("Invoice file not found.");

    var patientFromTo = $"{latestPatient.InvoiceStartDate:MMM d, yyyy} to {latestPatient.InvoiceEndDate:MMM d, yyyy}";
    var patientSubject = $"{patient.Name} - {patientFromTo} Invoice";
    var patientBody = $"Hello {patient.Name},\n\nPlease find your invoice attached ({patientFromTo}).\n\nBest regards,\nThe PMS Team";
    var patientSent = await _emailSenderService.SendEmailWithAttachment(patientToEmail, patient.Name ?? "Customer", patientSubject, patientBody, patientFullPath, Path.GetFileName(latestPatient.FilePath));
    if (patientSent)
    {
      latestPatient.IsSent = true;
      latestPatient.ModifiedDate = DateTime.UtcNow;
      InvoiceStatusHistoryHelper.AppendStatus(latestPatient, InvoiceStatusConstants.Sent);
      _appDbContext.InvoiceHistory.Update(latestPatient);
      await _appDbContext.SaveChangesAsync(cancellationToken);
    }
    return patientSent ? ApplicationResult<bool>.SuccessResult(true) : ApplicationResult<bool>.Error("Failed to send email.");
  }
}
