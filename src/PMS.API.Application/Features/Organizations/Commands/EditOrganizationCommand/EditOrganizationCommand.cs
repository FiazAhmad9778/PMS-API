using System.ComponentModel.DataAnnotations;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Organizations.Commands.EditOrganizationCommand;
public class EditOrganizationCommand : IRequest<ApplicationResult<long>>
{
  [Required]
  public long Id { get; set; } 

  [Required]
  public long OrganizationExternalId { get; set; }

  public long[]? WardIds { get; set; }

  public string? Name { get; set; }
  public string? Address { get; set; }
  public string? DefaultEmail { get; set; }
  public string? ContactName { get; set; }
  public int? MinimumThreshold { get; set; }
  public bool IsPatientRequired { get; set; }

  public List<string> CC { get; set; } = new();
}
public class EditOrganizationHandler
    : RequestHandlerBase<EditOrganizationCommand, ApplicationResult<long>>
{
  private readonly AppDbContext _dbContext;
  private readonly IConfiguration _configuration;

  public EditOrganizationHandler(
      AppDbContext dbContext,
      IConfiguration configuration,
      IServiceProvider serviceProvider,
      ILogger<EditOrganizationHandler> logger)
      : base(serviceProvider, logger)
  {
    _dbContext = dbContext;
    _configuration = configuration;
  }

  protected override async Task<ApplicationResult<long>> HandleRequest(
      EditOrganizationCommand request,
      CancellationToken cancellationToken)
  {
    try
    {
      var organization = await _dbContext.Organization
          .Include(o => o.Wards)
          .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

      if (organization == null)
        return ApplicationResult<long>.Error("Organization not found.");

      var sql = @"
                SELECT 
                    nh.ID as OrganizationId,
                    nh.Name as OrganizationName,
                    nh.Address1 as Address,
                    w.ID as WardId,
                    w.Name as WardName
                FROM dbo.NH nh
                LEFT JOIN dbo.NHWard w ON w.NHID = nh.ID
                WHERE nh.ID = @OrganizationExternalId";

      if (request.WardIds != null && request.WardIds.Length > 0)
        sql += " AND w.ID IN @WardIds";

      using var connection = new SqlConnection(
          _configuration.GetConnectionString("ARDashboardConnection")
          ?? throw new InvalidOperationException("Connection string not found"));

      await connection.OpenAsync(cancellationToken);

      var data = (await connection.QueryAsync<PmsOrgWardRow>(
          sql,
          new { request.OrganizationExternalId, request.WardIds }))
          .ToList();

      if (!data.Any())
        return ApplicationResult<long>.Error("No organization/wards found in PMS DB.");

      var firstRow = data.First();

      var wardsFromPms = data
          .Where(d => d.WardId.HasValue)
          .GroupBy(d => d.WardId!.Value)
          .Select(g => g.First())
          .ToList();

      organization.Name = firstRow.OrganizationName ?? request.Name ?? organization.Name;
      organization.Address = firstRow.Address ?? request.Address ?? organization.Address;
      organization.ContractEmail = string.IsNullOrWhiteSpace(request.DefaultEmail)
          ? null
          : request.DefaultEmail;

      organization.ContactName = request.ContactName;
      organization.MinimumThreshold = request.MinimumThreshold;
      organization.IsPatientRequired = request.IsPatientRequired;
      organization.CC = request.CC.Any()
          ? string.Join(",", request.CC)
          : string.Empty;

      organization.ModifiedDate = DateTime.UtcNow;

      // sync wards
      var existingWards = organization.Wards.ToList();
      var existingExternalIds = existingWards
          .Select(w => w.ExternalId)
          .ToHashSet();

      var pmsExternalIds = wardsFromPms
          .Select(w => w.WardId!.Value)
          .ToHashSet();

      AddWards(organization, wardsFromPms, existingExternalIds);

      foreach (var ward in existingWards)
      {
        var pmsWard = wardsFromPms
            .FirstOrDefault(w => w.WardId == ward.ExternalId);

        if (pmsWard != null && ward.Name != pmsWard.WardName)
        {
          ward.Name = pmsWard.WardName ?? ward.Name;
        }
      }

      RemoveWards(existingWards, pmsExternalIds);

      await _dbContext.SaveChangesAsync(cancellationToken);

      return ApplicationResult<long>.SuccessResult(organization.Id);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex,
          "Error editing organization {Id}", request.Id);

      return ApplicationResult<long>.Error("Failed to update organization.");
    }
  }

  private void RemoveWards(List<Ward> existingWards, HashSet<long> pmsExternalIds)
  {
    var wardsToRemove = existingWards
        .Where(w => !pmsExternalIds.Contains(w.ExternalId))
        .ToList();

    if (wardsToRemove.Any())
      _dbContext.Ward.RemoveRange(wardsToRemove);
  }

  private void AddWards(Organization? organization, List<PmsOrgWardRow> wardsFromPms, HashSet<long> existingExternalIds)
  {
    var wardsToAdd = wardsFromPms
        .Where(w => !existingExternalIds.Contains(w.WardId!.Value))
        .Select(w => new Ward
        {
          ExternalId = w.WardId!.Value,
          Name = w.WardName ?? string.Empty,
          OrganizationId = organization?.Id,
          CreatedDate = DateTime.UtcNow
        })
        .ToList();

    if (wardsToAdd.Any())
      _dbContext.Ward.AddRange(wardsToAdd);
  }
}
