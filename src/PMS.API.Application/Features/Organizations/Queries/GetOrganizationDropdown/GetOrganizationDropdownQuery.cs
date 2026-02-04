using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Organizations.Queries;
public class GetOrganizationDropdownQuery : IRequest<ApplicationResult<List<OrganizationDropdownDto>>>
{
  public string? SearchKeyword { get; set; }
}

public class GetOrganizationDropdownQueryHandler : RequestHandlerBase<GetOrganizationDropdownQuery, ApplicationResult<List<OrganizationDropdownDto>>>
{
  readonly AppDbContext _appDbContext;
  public GetOrganizationDropdownQueryHandler(IServiceProvider serviceProvider,
    ILogger<GetOrganizationDropdownQueryHandler> logger,
    AppDbContext appDbContext) : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<List<OrganizationDropdownDto>>> HandleRequest(
    GetOrganizationDropdownQuery request,
    CancellationToken cancellationToken)
  {
    var query = _appDbContext.Organization
        .AsNoTracking()
        .Where(x => !x.IsDeleted); 

    if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
    {
      var keyword = request.SearchKeyword.Trim().ToLower();
      query = query.Where(x => x.Name.ToLower().Contains(keyword));
    }

    query = query.OrderBy(x => x.Name);

    var organizations = await query
        .Select(x => new OrganizationDropdownDto
        {
          Id = x.Id,
          Name = x.Name
        })
        .ToListAsync(cancellationToken);

    if (!organizations.Any())
      return ApplicationResult<List<OrganizationDropdownDto>>.Error("No organizations found!");

    return ApplicationResult<List<OrganizationDropdownDto>>.SuccessResult(organizations, organizations.Count);
  }

}
