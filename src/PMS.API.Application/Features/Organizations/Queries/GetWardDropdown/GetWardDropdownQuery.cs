using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Organizations.Queries.GetWardDropdown;
public class GetWardDropdownQuery : IRequest<ApplicationResult<List<WardResponseDto>>>
{
  [Required]
  public long OrganizationId { get; set; }
}

public class GetWardDropdownQueryHandler : RequestHandlerBase<GetWardDropdownQuery, ApplicationResult<List<WardResponseDto>>>
{
  readonly AppDbContext _appDbContext;

  public GetWardDropdownQueryHandler(
    IServiceProvider serviceProvider,
    AppDbContext appDbContext,
    ILogger<GetWardDropdownFromPMSQueryHandler> logger) : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<List<WardResponseDto>>> HandleRequest(
    GetWardDropdownQuery request,
    CancellationToken cancellationToken)
  {
    if (request.OrganizationId <= 0)
      return ApplicationResult<List<WardResponseDto>>.Error("Invalid OrganizationId.");

    var query = _appDbContext.Ward
        .AsNoTracking()
        .Where(w => !w.IsDeleted && w.OrganizationId == request.OrganizationId);

    query = query.OrderBy(w => w.Name);

    var wards = await query
        .Select(w => new WardResponseDto
        {
          Id = w.Id,
          Name = w.Name,
          ExternalId = w.ExternalId
        })
        .ToListAsync(cancellationToken);

    if (!wards.Any())
      return ApplicationResult<List<WardResponseDto>>.Error("No wards found for this organization.");

    return ApplicationResult<List<WardResponseDto>>.SuccessResult(wards, wards.Count);
  }

}
