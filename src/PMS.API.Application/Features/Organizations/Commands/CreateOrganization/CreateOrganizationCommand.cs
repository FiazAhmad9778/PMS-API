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
using System.ComponentModel.DataAnnotations;

namespace PMS.API.Application.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommand : IRequest<ApplicationResult<long>>
{
  /// <summary>PMS (external) organization ID. Used to fetch org + wards from PMS and to link in our DB.</summary>
  [Required]
  public required long OrganizationExternalId { get; set; }

  /// <summary>PMS (external) ward IDs to link. Fetched from PMS and saved in our DB if they don't already exist for this org.</summary>
  public long[]? WardIds { get; set; }

  /// <summary>Optional; used when creating a new org if PMS name is missing.</summary>
  public string? Name { get; set; }

  public string? Address { get; set; }

  public string? DefaultEmail { get; set; }
}

public class CreateOrganizationHandler : RequestHandlerBase<CreateOrganizationCommand, ApplicationResult<long>>
{
  private readonly IConfiguration _configuration;
  private readonly AppDbContext _dbContext;

  public CreateOrganizationHandler(
    IConfiguration configuration,
    AppDbContext dbContext,
    IServiceProvider serviceProvider,
    ILogger<CreateOrganizationHandler> logger) : base(serviceProvider, logger)
  {
    _configuration = configuration;
    _dbContext = dbContext;
  }

  protected override async Task<ApplicationResult<long>> HandleRequest(
    CreateOrganizationCommand request,
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

      var wardsFromPms = data
          .Where(d => d.WardId.HasValue)
          .GroupBy(d => d.WardId!.Value)
          .Select(g => g.First())
          .ToList();

      var firstRow = data.First();

      var organization = await _dbContext.Organization
          .Include(o => o.Wards)
          .FirstOrDefaultAsync(
              o => o.OrganizationExternalId == request.OrganizationExternalId,
              cancellationToken);

      if (organization != null)
      {
        // 3a. Existing org → add missing wards
        var existingWardExternalIds = organization.Wards
            .Select(w => w.ExternalId)
            .ToHashSet();

        var newWards = wardsFromPms
            .Where(w => !existingWardExternalIds.Contains(w.WardId!.Value))
            .Select(w => new Ward
            {
              ExternalId = w.WardId!.Value,
              Name = w.WardName ?? string.Empty,
              OrganizationId = organization.Id,
              CreatedDate = DateTime.UtcNow
            })
            .ToList();

        if (newWards.Any())
        {
          _dbContext.Ward.AddRange(newWards);
          await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Reload wards to include newly inserted ones
        organization.Wards = await _dbContext.Ward
            .Where(w => w.OrganizationId == organization.Id)
            .ToListAsync(cancellationToken);

        // Sync patients
        await SyncPatientsAsync(organization, organization.Wards, cancellationToken);

        return ApplicationResult<long>.SuccessResult(organization.Id);
      }

      // New organization
      organization = new Organization
      {
        OrganizationExternalId = request.OrganizationExternalId,
        Name = firstRow.OrganizationName ?? request.Name ?? string.Empty,
        Address = firstRow.Address ?? request.Address ?? string.Empty,
        DefaultEmail = string.IsNullOrWhiteSpace(request.DefaultEmail)
              ? null
              : request.DefaultEmail,
        CreatedDate = DateTime.UtcNow
      };

      foreach (var row in wardsFromPms)
      {
        organization.Wards.Add(new Ward
        {
          ExternalId = row.WardId!.Value,
          Name = row.WardName ?? string.Empty,
          CreatedDate = DateTime.UtcNow
        });
      }

      _dbContext.Organization.Add(organization);
      await _dbContext.SaveChangesAsync(cancellationToken);

      await SyncPatientsAsync(organization, organization.Wards, cancellationToken);

      return ApplicationResult<long>.SuccessResult(organization.Id);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex,
          "Error saving organization (external {ExternalId})",
          request.OrganizationExternalId);

      return ApplicationResult<long>.Error("Failed to save organization and wards.");
    }
  }

  private async Task<List<PmsPatientRow>> FetchPatientsFromPms(
    IEnumerable<long> wardExternalIds,
    CancellationToken ct)
  {
    var sql = @"
        SELECT
            p.ID AS PatientExternalId,
            p.FirstName + ' ' + p.LastName AS PatientName,
            p.Email,
            p.Address1 AS Address,
            p.NHWardID AS WardExternalId
        FROM dbo.PAT p
        WHERE p.NHWardID IN @WardExternalIds";

    using var connection = new SqlConnection(
        _configuration.GetConnectionString("ARDashboardConnection"));

    await connection.OpenAsync(ct);

    return (await connection.QueryAsync<PmsPatientRow>(
        sql,
        new { WardExternalIds = wardExternalIds }))
        .ToList();
  }
  private async Task SyncPatientsAsync(
     Organization organization,
     List<Ward> wards,
     CancellationToken ct)
  {
    if (wards == null || !wards.Any())
      return;

    // 1. External ward IDs (PMS)
    var wardExternalIds = wards
        .Select(w => w.ExternalId)
        .ToList();

    // 2. Fetch patients from PMS
    var pmsPatients = await FetchPatientsFromPms(wardExternalIds, ct);
    if (!pmsPatients.Any())
      return;

    // 3. Map PMS WardExternalId → Local Ward
    var wardMap = wards.ToDictionary(w => w.ExternalId, w => w);

    // 4. Local ward IDs (nullable-safe)
    var localWardIds = wards.Select(w => w.Id).ToList();

    List<Patient> existingPatients = new List<Patient>();
    try
    {
     existingPatients = await _dbContext.Patient.Where(x => localWardIds.Contains(x.WardId.GetValueOrDefault())).ToListAsync();

    }
    catch (Exception)
    {
      throw;
    }

    // 6. Existing external patient IDs
    var existingPatientExternalIds = existingPatients
        .Select(p => p.PatientId)
        .ToHashSet();

    // 7. New patients to insert
    var newPatients = pmsPatients
        .Where(p => !existingPatientExternalIds.Contains(p.PatientExternalId))
        .Select(p => new Patient
        {
          PatientId = p.PatientExternalId,
          Name = p.PatientName,
          DefaultEmail = p.Email,
          Address = p.Address,
          WardId = wardMap[p.WardExternalId].Id,
          CreatedDate = DateTime.UtcNow
        })
        .ToList();

    // 8. Save
    if (newPatients.Any())
    {
      _dbContext.Patient.AddRange(newPatients);
      await _dbContext.SaveChangesAsync(ct);
    }
  }

}
