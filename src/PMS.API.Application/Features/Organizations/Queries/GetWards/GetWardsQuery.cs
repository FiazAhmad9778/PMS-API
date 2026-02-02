using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.DTOs.Common.Base.Request;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Organizations.Queries.GetWards;
public class GetWardsQuery : PagedQueryDTO, IRequest<ApplicationResult<List<WardPageResponseDTO>>>
{
  public int WardId { get; set; }
}

public class GetWardsQueryHandler : RequestHandlerBase<GetWardsQuery, ApplicationResult<List<WardPageResponseDTO>>>
{
  readonly AppDbContext _appDbContext;
  public GetWardsQueryHandler(
    IServiceProvider serviceProvider,
    ILogger<GetWardsQueryHandler> logger,
    AppDbContext appDbContext) : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<List<WardPageResponseDTO>>> HandleRequest(
    GetWardsQuery request,
    CancellationToken cancellationToken)
  {
    IQueryable<Ward> query = _appDbContext.Ward
        .AsNoTracking();

    if (request.WardId > 0)
    {
      query = query.Where(x => x.Id == request.WardId);
    }

    if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
    {
      var keyword = request.SearchKeyword.Trim();
      query = query.Where(x => x.Name.Contains(keyword));
    }

    if (!string.IsNullOrWhiteSpace(request.OrderBy))
    {
      var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    nameof(Ward.Id),
    nameof(Ward.ExternalId),
    nameof(Ward.OrganizationId),
    nameof(Ward.Name),
    nameof(Ward.CreatedDate),
    nameof(Ward.ModifiedDate)
};


      if (allowedSortColumns.Contains(request.OrderBy))
      {
        if (request.OrderBy == nameof(Ward.Name))
        {
          query = request.SortByAscending
              ? query.OrderBy(x => x.Name.ToLower())
              : query.OrderByDescending(x => x.Name.ToLower());
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
      query = query.OrderByDescending(x => x.CreatedDate); 
    }

    var totalCount = await query.CountAsync(cancellationToken);
    if (totalCount == 0)
    {
      return ApplicationResult<List<WardPageResponseDTO>>.Error("No wards found!");
    }

    if (request.PageSize > 0)
    {
      query = query
          .Skip((request.PageNumber - 1) * request.PageSize)
          .Take(request.PageSize);
    }

    var wards = await query
        .Select(x => new WardPageResponseDTO
        {
          Id = x.Id,
          Name = x.Name,
          ExternalId = x.ExternalId,
          OrganizationId = x.OrganizationId,
          OrganizationName = x.OrganizationId != null ? x.Organization!.Name : string.Empty,
          CreatedDate = x.CreatedDate,
          ModifiedDate = x.ModifiedDate
        })
        .ToListAsync(cancellationToken);

    return ApplicationResult<List<WardPageResponseDTO>>.SuccessResult(wards, totalCount);
  }

}
