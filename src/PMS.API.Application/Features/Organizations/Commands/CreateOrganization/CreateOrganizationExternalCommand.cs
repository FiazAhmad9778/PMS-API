using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PMS.API.Application.Common.Models;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Organizations.Commands.CreateOrganization;
public class CreateOrganizationExternalCommand : IRequest<ApplicationResult<bool>>
{
  public long OrganizationId { get; set; }
  public long[]? WardIds { get; set; }
}

public class CreateOrganizationExternalCommandHandler
    : IRequestHandler<CreateOrganizationExternalCommand, ApplicationResult<bool>>
{
  private readonly IConfiguration _configuration;
  private readonly AppDbContext _dbContext;

  public CreateOrganizationExternalCommandHandler(
      IConfiguration configuration,
      AppDbContext dbContext)
  {
    _configuration = configuration;
    _dbContext = dbContext;
  }

  public async Task<ApplicationResult<bool>> Handle(
      CreateOrganizationExternalCommand request,
      CancellationToken cancellationToken)
  {
    try
    {
      var sql = @"
                SELECT 
                    nh.ID as OrganizationId,
                    nh.Name as OrganizationName,
                    nh.Address1 as Address,
                    w.ID as WardId,
                    w.Name as WardName
                FROM dbo.NH nh
                LEFT JOIN dbo.NHWard w ON w.NHID = nh.ID
                WHERE nh.ID = @OrganizationId";

      if (request.WardIds != null && request.WardIds.Any())
      {
        sql += " AND w.ID IN @WardIds";
      }

      using var connection = new SqlConnection(
          _configuration.GetConnectionString("ARDashboardConnection"));

      await connection.OpenAsync(cancellationToken);

      var data = (await connection.QueryAsync<dynamic>(
          sql,
          new { OrganizationId = request.OrganizationId, WardIds = request.WardIds }))
          .ToList();

      if (!data.Any())
        return ApplicationResult<bool>.Error("No organization/wards found in PMS DB.");

      var organizationEntity = new Organization
      {
        OrganizationExternalId = request.OrganizationId,
        Name = data.FirstOrDefault()?.OrganizationName ?? string.Empty,
        Address = data.FirstOrDefault()?.Address ?? string.Empty,
        CreatedDate = DateTime.UtcNow,
      };

      foreach (var row in data)
      {
        if (row.WardId != null)
        {
          organizationEntity.Wards.Add(new Ward
          {
            ExternalId=Convert.ToString(row.WardId),
            Name = row.WardName,
            OrganizationId = organizationEntity.Id,
            CreatedDate = DateTime.UtcNow
          });
        }
      }

      _dbContext.Organization.Add(organizationEntity);
      await _dbContext.SaveChangesAsync(cancellationToken);

      return ApplicationResult<bool>.SuccessResult(true);
    }
    catch (Exception ex)
    {
      return ApplicationResult<bool>.Error(
          new[] { "Failed to save organization and wards." }, ex);
    }
  }
}

