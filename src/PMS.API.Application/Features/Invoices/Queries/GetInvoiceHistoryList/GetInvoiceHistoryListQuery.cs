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

    List<long> externalOrgIds = new List<long>();
    List<long> externalPatientIds = new List<long>();

    if (hasOrgFilter)
      externalOrgIds = await _appDbContext.Organization
        .Where(o => !o.IsDeleted && request.OrganizationIds!.Contains(o.Id))
        .Select(o => o.OrganizationExternalId)
        .ToListAsync(cancellationToken);

    if (hasPatientFilter)
      externalPatientIds = await _appDbContext.Patient
        .Where(p => !p.IsDeleted && request.PatientIds!.Contains(p.Id))
        .Select(p => p.PatientId)
        .ToListAsync(cancellationToken);

    var query = _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => !h.IsDeleted);

    if (hasOrgFilter && hasPatientFilter)
      query = query.Where(h =>
        (h.OrganizationId != null && externalOrgIds.Contains(h.OrganizationId.Value)) ||
        (h.PatientId != null && externalPatientIds.Contains(h.PatientId.Value)));
    else if (hasOrgFilter)
      query = query.Where(h => h.OrganizationId != null && externalOrgIds.Contains(h.OrganizationId.Value));
    else if (hasPatientFilter)
      query = query.Where(h => h.PatientId != null && externalPatientIds.Contains(h.PatientId.Value));
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

    var orgExternalIds = list.Where(x => x.OrganizationId.HasValue).Select(x => x.OrganizationId!.Value).Distinct().ToList();
    var patientExternalIds = list.Where(x => x.PatientId.HasValue).Select(x => x.PatientId!.Value).Distinct().ToList();

    var orgMap = await _appDbContext.Organization
      .Where(o => !o.IsDeleted && orgExternalIds.Contains(o.OrganizationExternalId))
      .Select(o => new { o.OrganizationExternalId, o.Id })
      .ToListAsync(cancellationToken);
    var orgLookup = orgMap.ToDictionary(x => x.OrganizationExternalId, x => x.Id);

    var patientMap = await _appDbContext.Patient
      .Where(p => !p.IsDeleted && patientExternalIds.Contains(p.PatientId))
      .Select(p => new { p.PatientId, p.Id })
      .ToListAsync(cancellationToken);
    var patientLookup = patientMap.ToDictionary(x => x.PatientId, x => x.Id);

    var dtos = list.Select(h =>
    {
      var isOrg = h.OrganizationId.HasValue;
      return new InvoiceHistoryItemDto
      {
        Id = h.Id,
        InvoiceType = isOrg ? "Organization" : "Patient",
        OrganizationInternalId = h.OrganizationId.HasValue && orgLookup.TryGetValue(h.OrganizationId.Value, out var oid) ? oid : null,
        PatientInternalId = h.PatientId.HasValue && patientLookup.TryGetValue(h.PatientId.Value, out var pid) ? pid : null,
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
