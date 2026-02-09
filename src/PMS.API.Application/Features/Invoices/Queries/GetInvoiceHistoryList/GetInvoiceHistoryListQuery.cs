using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Invoices.DTO;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Invoices.Queries.GetInvoiceHistoryList;

public class GetInvoiceHistoryListQuery : IRequest<ApplicationResult<List<InvoiceHistoryItemDto>>>
{
  /// <summary>Filter by internal organization IDs (Organization.Id).</summary>
  public List<long>? OrganizationIds { get; set; }
  /// <summary>Filter by internal patient IDs (Patient.Id).</summary>
  public List<long>? PatientIds { get; set; }
}

public class GetInvoiceHistoryListQueryHandler : RequestHandlerBase<GetInvoiceHistoryListQuery, ApplicationResult<List<InvoiceHistoryItemDto>>>
{
  private readonly AppDbContext _appDbContext;

  public GetInvoiceHistoryListQueryHandler(
    IServiceProvider serviceProvider,
    ILogger<GetInvoiceHistoryListQueryHandler> logger,
    AppDbContext appDbContext)
    : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<List<InvoiceHistoryItemDto>>> HandleRequest(
    GetInvoiceHistoryListQuery request,
    CancellationToken cancellationToken)
  {
    var hasOrgFilter = request.OrganizationIds != null && request.OrganizationIds.Count > 0;
    var hasPatientFilter = request.PatientIds != null && request.PatientIds.Count > 0;

    var query = _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => !h.IsDeleted);

    if (hasOrgFilter && hasPatientFilter)
      query = query.Where(h =>
        (h.OrganizationId != null && request.OrganizationIds!.Contains(h.OrganizationId.Value)) ||
        (h.PatientId != null && request.PatientIds!.Contains(h.PatientId.Value)));
    else if (hasOrgFilter)
      query = query.Where(h => h.OrganizationId != null && request.OrganizationIds!.Contains(h.OrganizationId.Value));
    else if (hasPatientFilter)
      query = query.Where(h => h.PatientId != null && request.PatientIds!.Contains(h.PatientId.Value));
    // else: no filter = return all

    var list = await query
      .OrderByDescending(h => h.CreatedDate)
      .Select(h => new
      {
        h.Id,
        h.OrganizationId,
        h.PatientId,
        h.InvoiceStatus,
        h.FilePath,
        h.InvoiceStartDate,
        h.InvoiceEndDate,
        h.CreatedDate,
        h.IsSent
      })
      .ToListAsync(cancellationToken);

    if (list.Count == 0)
      return ApplicationResult<List<InvoiceHistoryItemDto>>.SuccessResult(new List<InvoiceHistoryItemDto>(), 0);

    var orgInternalIds = list.Where(x => x.OrganizationId.HasValue).Select(x => x.OrganizationId!.Value).Distinct().ToList();
    var patientInternalIds = list.Where(x => x.PatientId.HasValue).Select(x => x.PatientId!.Value).Distinct().ToList();

    var orgNameMap = orgInternalIds.Count > 0
      ? await _appDbContext.Organization
          .Where(o => !o.IsDeleted && orgInternalIds.Contains(o.Id))
          .ToDictionaryAsync(o => o.Id, o => o.Name, cancellationToken)
      : new Dictionary<long, string>();

    var patientNameMap = patientInternalIds.Count > 0
      ? await _appDbContext.Patient
          .Where(p => !p.IsDeleted && patientInternalIds.Contains(p.Id))
          .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken)
      : new Dictionary<long, string>();

    var dtos = list.Select(h =>
    {
      var isOrg = h.OrganizationId.HasValue;
      var name = isOrg
        ? (h.OrganizationId.HasValue && orgNameMap.TryGetValue(h.OrganizationId.Value, out var on) ? on : null)
        : (h.PatientId.HasValue && patientNameMap.TryGetValue(h.PatientId.Value, out var pn) ? pn : null);
      return new InvoiceHistoryItemDto
      {
        Id = h.Id,
        InvoiceType = isOrg ? "Organization" : "Patient",
        Name = name,
        InvoiceStatus = h.InvoiceStatus,
        FilePath = h.FilePath,
        DownloadUrl = string.IsNullOrEmpty(h.FilePath) ? null : $"api/Invoice/download/{h.Id}",
        InvoiceStartDate = h.InvoiceStartDate,
        InvoiceEndDate = h.InvoiceEndDate,
        CreatedDate = h.CreatedDate,
        IsSent = h.IsSent
      };
    }).ToList();

    return ApplicationResult<List<InvoiceHistoryItemDto>>.SuccessResult(dtos, dtos.Count);
  }
}
