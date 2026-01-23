using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.DTOs.Base;
using PMS.API.Core.Extensions;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.ARDashboard.Queries.GetARDashboardOrganizations;

public class GetARDashboardOrganizationsQuery : PagedQueryBaseRequest, IRequest<ApplicationResult<List<OrganizationResponseDto>>>
{
}

public class GetARDashboardOrganizationsQueryHandler : RequestHandlerBase<GetARDashboardOrganizationsQuery, ApplicationResult<List<OrganizationResponseDto>>>
{
  private readonly AppDbContext _dbContext;

  public GetARDashboardOrganizationsQueryHandler(
    AppDbContext dbContext,
    IServiceProvider serviceProvider,
    ILogger<GetARDashboardOrganizationsQueryHandler> logger) : base(serviceProvider, logger)
  {
    _dbContext = dbContext;
  }

  protected override async Task<ApplicationResult<List<OrganizationResponseDto>>> HandleRequest(GetARDashboardOrganizationsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var searchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword.Trim().ToLower() : "";
      
      IQueryable<Organization> organizations = _dbContext.Organization.AsNoTracking();

      // Filter by search keyword - search by organization name
      if (!string.IsNullOrEmpty(searchKeyword))
      {
        organizations = organizations.Where(o => o.Name.ToLower().Contains(searchKeyword));
      }

      // Get total count
      int totalCount = await organizations.CountAsync(cancellationToken);

      // Apply sorting and pagination
      IQueryable<Organization> orderedOrganizations = request.SortByAscending
        ? organizations.OrderBy(o => o.Name)
        : organizations.OrderByDescending(o => o.Name);

      var organizationList = await orderedOrganizations
        .Include(o => o.Wards)
        .Paginate(request.PageNumber, request.PageSize)
        .ToListAsync(cancellationToken);

      // Map to DTO
      var result = organizationList.Select(org => new OrganizationResponseDto
      {
        Id = org.Id,
        Name = org.Name,
        WardIds = org.Wards.Select(w => w.Id).ToArray(), // Get WardIds from navigation property
        Address = org.Address,
        DefaultEmail = org.DefaultEmail,
        CreatedDate = org.CreatedDate
      }).ToList();

      return ApplicationResult<List<OrganizationResponseDto>>.SuccessResult(result, totalCount);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching organizations for AR Dashboard");
      return ApplicationResult<List<OrganizationResponseDto>>.Error($"Error fetching organizations: {ex.Message}");
    }
  }
}

