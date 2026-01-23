using System.Data;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Patients.Commands.AddPatientsByFilter;

public class AddPatientsByFilterCommand : IRequest<ApplicationResult<int>>
{
  public required long OrganizationId { get; set; }
}

public class AddPatientsByFilterHandler : RequestHandlerBase<AddPatientsByFilterCommand, ApplicationResult<int>>
{
  private readonly SharedKernel.Interfaces.IRepository<Patient> _patientRepository;
  private readonly AppDbContext _dbContext;
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;

  public AddPatientsByFilterHandler(
    SharedKernel.Interfaces.IRepository<Patient> patientRepository,
    AppDbContext dbContext,
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<AddPatientsByFilterHandler> logger) : base(serviceProvider, logger)
  {
    _patientRepository = patientRepository;
    _dbContext = dbContext;
    _configuration = configuration;
    _connectionString = _configuration.GetConnectionString("ARDashboardConnection")
      ?? throw new InvalidOperationException("Connection string 'ARDashboardConnection' not found.");
    _databaseName = ExtractDatabaseName(_connectionString);
  }

  private string ExtractDatabaseName(string connectionString)
  {
    var dbIndex = connectionString.IndexOf("Database=", StringComparison.OrdinalIgnoreCase);
    if (dbIndex == -1) return "Kroll"; // Fallback to default
    
    var startIndex = dbIndex + "Database=".Length;
    var endIndex = connectionString.IndexOf(";", startIndex);
    if (endIndex == -1) endIndex = connectionString.Length;
    
    return connectionString.Substring(startIndex, endIndex - startIndex).Trim();
  }

  protected override async Task<ApplicationResult<int>> HandleRequest(AddPatientsByFilterCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Query SQL Server for patients with matching organization ID
      var patientsQuery = $@"
        SELECT DISTINCT
          pat.ARID as PatientId,
          CONCAT(pat.LastName, ', ', pat.FirstName) as Name,
          pat.NHID as OrganizationId,
          NULL as Address,
          NULL as DefaultEmail,
          'active' as Status,
          pat.LastUsed as CreatedDate
        FROM (
          SELECT a.* 
          FROM (
            SELECT *, 
              ROW_NUMBER() OVER (PARTITION BY ARID ORDER BY LastUsed DESC) rnk 
            FROM [{_databaseName}].dbo.Pat
          ) a 
          WHERE rnk = 1
        ) pat
        LEFT JOIN [{_databaseName}].dbo.NH nh ON nh.ID = pat.NHID
        WHERE pat.NHID = @OrganizationId";

      var patientsToAdd = new List<Patient>();

      // Get ALL existing patient IDs from PostgreSQL to avoid duplicates
      var allExistingPatientIds = await _dbContext.Patient
        .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.PatientId))
        .Select(p => p.PatientId!)
        .ToHashSetAsync(cancellationToken);

      // Query SQL Server and collect patients to add
      using (var connection = new SqlConnection(_connectionString))
      {
        await connection.OpenAsync(cancellationToken);

        using (var command = new SqlCommand(patientsQuery, connection))
        {
          command.Parameters.AddWithValue("@OrganizationId", request.OrganizationId);

          using (var reader = await command.ExecuteReaderAsync(cancellationToken))
          {
            while (await reader.ReadAsync(cancellationToken))
            {
              var patientId = reader.IsDBNull("PatientId") ? null : reader.GetInt32("PatientId").ToString();

              // Skip if patient already exists in PostgreSQL
              if (string.IsNullOrEmpty(patientId) || allExistingPatientIds.Contains(patientId))
              {
                continue;
              }

              var patient = new Patient
              {
                PatientId = patientId, // External ID from Kroll database (ARID)
                Name = reader.IsDBNull("Name") ? string.Empty : reader.GetString("Name"),
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                DefaultEmail = reader.IsDBNull("DefaultEmail") ? null : reader.GetString("DefaultEmail"),
                Status = reader.IsDBNull("Status") ? "active" : reader.GetString("Status"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.UtcNow : reader.GetDateTime("CreatedDate"),
                IsDeleted = false
              };

              patientsToAdd.Add(patient);
              allExistingPatientIds.Add(patientId); // Track to avoid duplicates in same batch
            }
          }
        }
      }

      // Add all patients to PostgreSQL in batch
      if (patientsToAdd.Count > 0)
      {
        await _patientRepository.AddRangeAsync(patientsToAdd);
        await _dbContext.SaveChangesAsync(cancellationToken);
        Logger.LogInformation($"Added {patientsToAdd.Count} patients from SQL Server for organization ID {request.OrganizationId}");
      }
      else
      {
        Logger.LogInformation($"No new patients to add for organization ID {request.OrganizationId} (all patients already exist in PostgreSQL)");
      }

      return ApplicationResult<int>.SuccessResult(patientsToAdd.Count);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, $"Error adding patients by filter: {ex.Message}");
      return ApplicationResult<int>.Error($"Error adding patients: {ex.Message}");
    }
  }
}

