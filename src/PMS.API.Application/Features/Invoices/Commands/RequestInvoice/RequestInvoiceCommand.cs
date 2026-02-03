using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Constants;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;
using PMS.API.Infrastructure.Interfaces;

namespace PMS.API.Application.Features.Invoices.Commands.RequestInvoice;

public class RequestInvoiceCommand : IRequest<ApplicationResult<bool>>
{
  public List<long>? OrganizationIds { get; set; }
  public List<long>? PatientIds { get; set; }

  [Required]
  public DateTime FromDate { get; set; }

  [Required]
  public DateTime ToDate { get; set; }

  public bool IsSent { get; set; }
}

public class RequestInvoiceCommandHandler : RequestHandlerBase<RequestInvoiceCommand, ApplicationResult<bool>>
{
  readonly AppDbContext _appDbContext;
  readonly ICurrentUserService _currentUserService;

  public RequestInvoiceCommandHandler(
   IServiceProvider serviceProvider,
   ILogger<RequestInvoiceCommandHandler> logger,
   AppDbContext appDbContext,
   ICurrentUserService currentUserService)
   : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
    _currentUserService = currentUserService;
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(
     RequestInvoiceCommand request,
     CancellationToken cancellationToken)
  {
    if (request.FromDate > request.ToDate)
      return ApplicationResult<bool>.Error("FromDate cannot be greater than ToDate.");

    if ((request.OrganizationIds == null || !request.OrganizationIds.Any()) &&
        (request.PatientIds == null || !request.PatientIds.Any()))
      return ApplicationResult<bool>.Error("OrganizationIds or PatientIds required.");

    if (request.OrganizationIds?.Any() == true)
    {
      foreach (var organizationId in request.OrganizationIds)
      {
        await ProcessOrganizationInvoice(
            organizationId, request, cancellationToken);
      }
    }
    else
    {
      foreach (var patientId in request.PatientIds!)
      {
        await ProcessPatientInvoice(
            patientId, request, cancellationToken);
      }
    }

    return ApplicationResult<bool>.SuccessResult(true);
  }

  private async Task ProcessOrganizationInvoice(
    long organizationId,
    RequestInvoiceCommand request,
    CancellationToken ct)
  {
    var externalOrganizationId = await _appDbContext.Organization.Where(x => x.OrganizationExternalId == organizationId).Select(x => x.Id).FirstOrDefaultAsync();
    var wardIds = await _appDbContext.Ward
        .Where(w => w.OrganizationId == externalOrganizationId)
        .Select(w => w.ExternalId)
        .ToListAsync(ct);

    if (!wardIds.Any())
      return;

    var exists = await _appDbContext.InvoiceHistory
        .AnyAsync(h =>
            !h.IsDeleted &&
            h.OrganizationId == organizationId &&
            h.InvoiceStartDate <= request.ToDate &&
            h.InvoiceEndDate >= request.FromDate,
            ct);

    if (exists)
      throw new InvalidOperationException("Invoice already exists.");

    var history = new InvoiceHistory
    {
      OrganizationId = organizationId,
      PatientId = null,
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
    await _appDbContext.SaveChangesAsync(ct);
  }

  private async Task ProcessPatientInvoice(
    long patientId,
    RequestInvoiceCommand request,
    CancellationToken ct)
  {
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
    await _appDbContext.SaveChangesAsync(ct);
  }
}
