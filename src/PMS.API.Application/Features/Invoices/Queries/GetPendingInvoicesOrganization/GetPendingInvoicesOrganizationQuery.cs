using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Invoices.Queries.GetPendingInvoicesOrganization;

/// <summary>
/// Returns organizations that have a latest invoice generated (has file) but not sent, with pagination.
/// </summary>
public class GetPendingInvoicesOrganizationQuery : IRequest<ApplicationResult<List<OrganizationResponseDto>>>
{
  public int PageNumber { get; set; } = 1;
  public int PageSize { get; set; } = 50;
  public string? SearchKeyword { get; set; }
}

public class GetPendingInvoicesOrganizationQueryHandler : RequestHandlerBase<GetPendingInvoicesOrganizationQuery, ApplicationResult<List<OrganizationResponseDto>>>
{
  private readonly AppDbContext _appDbContext;

  public GetPendingInvoicesOrganizationQueryHandler(
    IServiceProvider serviceProvider,
    ILogger<GetPendingInvoicesOrganizationQueryHandler> logger,
    AppDbContext appDbContext)
    : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<List<OrganizationResponseDto>>> HandleRequest(
    GetPendingInvoicesOrganizationQuery request,
    CancellationToken cancellationToken)
  {
    // Latest CreatedDate per org (has file, not deleted)
    var latestPerOrg = _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => !h.IsDeleted && h.OrganizationId != null && h.FilePath != null && h.FilePath != "")
      .GroupBy(h => h.OrganizationId!.Value)
      .Select(g => new { OrganizationId = g.Key, MaxCreated = g.Max(h => h.CreatedDate) });

    // Org IDs where that latest invoice is not sent
    var pendingOrgIds = await _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => !h.IsDeleted && h.OrganizationId != null && !h.IsSent && h.FilePath != null && h.FilePath != "")
      .Where(h => latestPerOrg.Any(l => l.OrganizationId == h.OrganizationId!.Value && l.MaxCreated == h.CreatedDate))
      .Select(h => h.OrganizationId!.Value)
      .Distinct()
      .ToListAsync(cancellationToken);

    if (pendingOrgIds.Count == 0)
      return ApplicationResult<List<OrganizationResponseDto>>.SuccessResult(new List<OrganizationResponseDto>(), 0);

    var orgQuery = _appDbContext.Organization
      .AsNoTracking()
      .Where(o => !o.IsDeleted && pendingOrgIds.Contains(o.Id));

    if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
    {
      var kw = request.SearchKeyword.Trim();
      orgQuery = orgQuery.Where(o =>
        (o.Name != null && o.Name.Contains(kw)) ||
        (o.Address != null && o.Address.Contains(kw)) ||
        (o.DefaultEmail != null && o.DefaultEmail.Contains(kw)));
    }

    var totalCount = await orgQuery.CountAsync(cancellationToken);

    var organizations = await orgQuery
      .OrderBy(o => o.Name)
      .Skip((request.PageNumber - 1) * request.PageSize)
      .Take(request.PageSize)
      .Select(x => new OrganizationResponseDto
      {
        Id = x.Id,
        OrganizationExternalId = x.OrganizationExternalId,
        Name = x.Name,
        Address = x.Address,
        DefaultEmail = x.DefaultEmail,
        CreatedDate = x.CreatedDate,
        ModifiedDate = x.ModifiedDate,
        WardIds = x.Wards.Select(w => w.Id).ToArray(),
        LastInvoiceId = x.InvoiceHistoryList
          .Where(h => h.OrganizationId == x.Id)
          .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
          .Select(h => (long?)h.Id)
          .FirstOrDefault(),
        InvoiceFromDate = x.InvoiceHistoryList
          .Where(h => h.OrganizationId == x.Id)
          .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
          .Select(h => (DateTime?)h.InvoiceStartDate)
          .FirstOrDefault(),
        InvoiceToDate = x.InvoiceHistoryList
          .Where(h => h.OrganizationId == x.Id)
          .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
          .Select(h => (DateTime?)h.InvoiceEndDate)
          .FirstOrDefault(),
        InvoiceIsSent = false,
        Wards = x.Wards.Select(w => new WardResponseDto
        {
          Id = w.Id,
          ExternalId = w.ExternalId,
          Name = w.Name
        }).ToList()
      })
      .ToListAsync(cancellationToken);

    return ApplicationResult<List<OrganizationResponseDto>>.SuccessResult(organizations, totalCount);
  }
}
