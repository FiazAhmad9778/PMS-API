using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.DTOs.Base;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Organizations.Queries.GetOrganizations;
public class GetOrganizationsQuery : PagedQueryBaseRequest, IRequest<ApplicationResult<List<OrganizationResponseDto>>>
{
}

public class GetOrganizationsQueryHandler : RequestHandlerBase<GetOrganizationsQuery, ApplicationResult<List<OrganizationResponseDto>>>
{
  readonly AppDbContext _appDbContext;
  public GetOrganizationsQueryHandler(IServiceProvider serviceProvider,
     ILogger<GetOrganizationsQueryHandler> logger,
    AppDbContext appDbContext) : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<List<OrganizationResponseDto>>> HandleRequest(
    GetOrganizationsQuery request,
    CancellationToken cancellationToken)
  {
    IQueryable<Organization> query = _appDbContext.Organization
        .AsNoTracking()
        .Where(x => !x.IsDeleted);

    if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
    {
      query = query.Where(x =>
          x.Name.Contains(request.SearchKeyword) ||
          x.Address.Contains(request.SearchKeyword) ||
          x.DefaultEmail!.Contains(request.SearchKeyword));
    }

    if (request.OrganizationId is not null)
    {
      query = query.Where(x => x.Id == request.OrganizationId);
    }

    if (request.OrganizationExternalId is not null)
    {
      query = query.Where(x => x.OrganizationExternalId == request.OrganizationExternalId);
    }

    if (!string.IsNullOrWhiteSpace(request.Ward))
    {
      var wardIds = request.Ward
          .Split(',', StringSplitOptions.RemoveEmptyEntries)
          .Select(w => w.Trim())
          .Where(w => long.TryParse(w, out _))
          .Select(long.Parse)
          .ToList();

      if (wardIds.Any())
      {
        query = query.Where(x => wardIds.All(id => x.Wards.Any(w => w.Id == id)));
      }
    }

    if (!string.IsNullOrWhiteSpace(request.OrderBy))
    {
      var allowedSortColumns = new HashSet<string>
    {
        nameof(Organization.Id),
        nameof(Organization.OrganizationExternalId),
        nameof(Organization.Name),
        nameof(Organization.Address),
        nameof(Organization.CreatedDate),
        nameof(Organization.ModifiedDate)
    };

      if (allowedSortColumns.Contains(request.OrderBy))
      {
        if (request.OrderBy == nameof(Organization.Name))
        {
          query = request.SortByAscending
              ? query.OrderBy(x => x.Name.ToLower())
              : query.OrderByDescending(x => x.Name.ToLower());
        }
        else if (request.OrderBy == nameof(Organization.Address))
        {
          query = request.SortByAscending
              ? query.OrderBy(x => x.Address.ToLower())
              : query.OrderByDescending(x => x.Address.ToLower());
        }
        else
        {
          query = request.SortByAscending
              ? query.OrderBy(x => EF.Property<object>(x, request.OrderBy))
              : query.OrderByDescending(x => EF.Property<object>(x, request.OrderBy));
        }
      }
    }
    else
    {
      query = query.OrderBy(x => x.Id);
    }


    var totalCount = await query.CountAsync(cancellationToken);

    if (totalCount == 0)
    {
      return ApplicationResult<List<OrganizationResponseDto>>
          .Error("Organization Does not Exist!");
    }

    if (request.PageSize > 0)
    {
      query = query
          .Skip((request.PageNumber - 1) * request.PageSize)
          .Take(request.PageSize);
    }

    var organizations = await query
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
          // Latest invoice by last created (CreatedDate), not by invoice period from/to
          InvoicePath = x.InvoiceHistoryList.Where(h => h.OrganizationId == x.Id)
                        .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id).Select(h => h.FilePath).FirstOrDefault() ?? string.Empty,
          InvoiceIsSent = x.InvoiceHistoryList.Where(h => h.OrganizationId == x.Id)
                        .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id).Select(h => (bool?)h.IsSent).FirstOrDefault(),
          InvoiceFromDate = x.InvoiceHistoryList.Where(h => h.OrganizationId == x.Id)
                        .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id).Select(h => (DateTime?)h.InvoiceStartDate).FirstOrDefault(),
          InvoiceToDate = x.InvoiceHistoryList.Where(h => h.OrganizationId == x.Id)
                        .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id).Select(h => (DateTime?)h.InvoiceEndDate).FirstOrDefault(),
          Wards = x.Wards.Select(w => new WardResponseDto
          {
            Id = w.Id,
            ExternalId = w.ExternalId,
            Name = w.Name
          }).ToList()
        })
        .ToListAsync(cancellationToken);

    return ApplicationResult<List<OrganizationResponseDto>>
        .SuccessResult(organizations, totalCount);
  }



}
