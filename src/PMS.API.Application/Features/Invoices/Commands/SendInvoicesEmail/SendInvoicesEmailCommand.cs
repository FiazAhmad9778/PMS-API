using System.IO;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Constants;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Invoices.Commands.SendInvoicesEmail;

/// <summary>
/// Sends invoice emails with the document attached using the existing SMTP (same as password reset).
/// For each org/patient, the latest generated invoice file is attached and sent to DefaultEmail; then marked IsSent = true.
/// </summary>
public class SendInvoicesEmailCommand : IRequest<ApplicationResult<bool>>
{
  /// <summary>Internal organization IDs (Organization.Id).</summary>
  public List<long>? OrganizationIds { get; set; }
  /// <summary>Internal patient IDs (Patient.Id).</summary>
  public List<long>? PatientIds { get; set; }
  /// <summary>Web root path (e.g. from IWebHostEnvironment.WebRootPath) to resolve invoice file paths.</summary>
  public string? WebRootPath { get; set; }
}

public class SendInvoicesEmailCommandHandler : RequestHandlerBase<SendInvoicesEmailCommand, ApplicationResult<bool>>
{
  private readonly AppDbContext _appDbContext;
  private readonly IEmailSenderService _emailSenderService;

  public SendInvoicesEmailCommandHandler(
    IServiceProvider serviceProvider,
    ILogger<SendInvoicesEmailCommandHandler> logger,
    AppDbContext appDbContext,
    IEmailSenderService emailSenderService)
    : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
    _emailSenderService = emailSenderService;
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(
    SendInvoicesEmailCommand request,
    CancellationToken cancellationToken)
  {
    if ((request.OrganizationIds == null || request.OrganizationIds.Count == 0) &&
        (request.PatientIds == null || request.PatientIds.Count == 0))
      return ApplicationResult<bool>.Error("OrganizationIds or PatientIds required.");

    var basePath = string.IsNullOrEmpty(request.WebRootPath) ? "wwwroot" : request.WebRootPath;

    if (request.OrganizationIds != null && request.OrganizationIds.Count > 0)
    {
      foreach (var orgId in request.OrganizationIds)
      {
        // Latest by last created (CreatedDate), not by invoice period from/to
        var latest = await _appDbContext.InvoiceHistory
          .Where(h => !h.IsDeleted && h.OrganizationId == orgId && h.FilePath != null && h.FilePath != "")
          .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
          .FirstOrDefaultAsync(cancellationToken);
        if (latest == null || latest.IsSent) continue;

        var org = await _appDbContext.Organization
          .AsNoTracking()
          .Where(o => o.Id == orgId)
          .Select(o => new { o.Name, o.DefaultEmail })
          .FirstOrDefaultAsync(cancellationToken);
        if (org == null) continue;

        var toEmail = org.DefaultEmail?.Trim();
        if (string.IsNullOrEmpty(toEmail))
        {
          Logger.LogWarning("Organization Id {OrgId} ({Name}) has no DefaultEmail; skipping send.", orgId, org.Name);
          continue;
        }

        var fullPath = Path.Combine(basePath, latest.FilePath!);
        if (!File.Exists(fullPath))
        {
          Logger.LogWarning("Invoice file not found: {Path}", fullPath);
          continue;
        }

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
      }
    }

    if (request.PatientIds != null && request.PatientIds.Count > 0)
    {
      foreach (var patientId in request.PatientIds)
      {
        // Latest by last created (CreatedDate), not by invoice period from/to
        var latest = await _appDbContext.InvoiceHistory
          .Where(h => !h.IsDeleted && h.PatientId == patientId && h.FilePath != null && h.FilePath != "")
          .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
          .FirstOrDefaultAsync(cancellationToken);
        if (latest == null || latest.IsSent) continue;

        var patient = await _appDbContext.Patient
          .AsNoTracking()
          .Where(p => p.Id == patientId)
          .Select(p => new { p.Name, p.DefaultEmail })
          .FirstOrDefaultAsync(cancellationToken);
        if (patient == null) continue;

        var toEmail = patient.DefaultEmail?.Trim();
        if (string.IsNullOrEmpty(toEmail))
        {
          Logger.LogWarning("Patient Id {PatientId} ({Name}) has no DefaultEmail; skipping send.", patientId, patient.Name);
          continue;
        }

        var fullPath = Path.Combine(basePath, latest.FilePath!);
        if (!File.Exists(fullPath))
        {
          Logger.LogWarning("Invoice file not found: {Path}", fullPath);
          continue;
        }

        var fromTo = $"{latest.InvoiceStartDate:MMM d, yyyy} to {latest.InvoiceEndDate:MMM d, yyyy}";
        var subject = $"{patient.Name} - {fromTo} Invoice";
        var body = $"Hello {patient.Name},\n\nPlease find your invoice attached ({fromTo}).\n\nBest regards,\nThe PMS Team";
        var sent = await _emailSenderService.SendEmailWithAttachment(toEmail, patient.Name ?? "Customer", subject, body, fullPath, Path.GetFileName(latest.FilePath));
        if (sent)
        {
          latest.IsSent = true;
          latest.ModifiedDate = DateTime.UtcNow;
          InvoiceStatusHistoryHelper.AppendStatus(latest, InvoiceStatusConstants.Sent);
          _appDbContext.InvoiceHistory.Update(latest);
          await _appDbContext.SaveChangesAsync(cancellationToken);
        }
      }
    }

    return ApplicationResult<bool>.SuccessResult(true);
  }
}
