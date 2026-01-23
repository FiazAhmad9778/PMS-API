using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.DTO;
using System.Data;

namespace PMS.API.Application.Features.Patients.Queries.GetPatientDropdown;

public class GetPatientDropdownQuery : IRequest<ApplicationResult<List<PatientResponseDto>>>
{
  public string? SearchKeyword { get; set; }
}

public class GetPatientDropdownQueryHandler : RequestHandlerBase<GetPatientDropdownQuery, ApplicationResult<List<PatientResponseDto>>>
{
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;

  public GetPatientDropdownQueryHandler(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<GetPatientDropdownQueryHandler> logger) : base(serviceProvider, logger)
  {
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

  protected override async Task<ApplicationResult<List<PatientResponseDto>>> HandleRequest(GetPatientDropdownQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var searchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword.Trim() : "";
      var hasSearch = !string.IsNullOrEmpty(searchKeyword);
      var searchPattern = hasSearch ? $"%{searchKeyword}%" : "";

      // Query to get distinct patients (no pagination for dropdown, but returns full data)
      var query = $@"
        SELECT DISTINCT
          pat.ARID as Id,
          pat.ARID as PatientId,
          CONCAT(pat.LastName, ', ', pat.FirstName) as Name,
          pat.LastName,
          pat.FirstName,
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
        WHERE 1=1";

      // Add search condition if needed - search by patient name
      if (hasSearch)
      {
        query += @"
          AND CONCAT(pat.LastName, ', ', pat.FirstName) LIKE @SearchKeyword";
      }

      query += @"
        ORDER BY pat.LastName ASC, pat.FirstName ASC";

      var patients = new List<PatientResponseDto>();

      using (var connection = new SqlConnection(_connectionString))
      {
        await connection.OpenAsync(cancellationToken);

        using (var command = new SqlCommand(query, connection))
        {
          if (hasSearch)
          {
            command.Parameters.AddWithValue("@SearchKeyword", searchPattern);
          }

          using (var reader = await command.ExecuteReaderAsync(cancellationToken))
          {
            while (await reader.ReadAsync(cancellationToken))
            {
              var id = reader.IsDBNull("Id") ? 0 : reader.GetInt32("Id");
              var patientIdValue = reader.IsDBNull("PatientId") ? id.ToString() : reader.GetInt32("PatientId").ToString();
              patients.Add(new PatientResponseDto
              {
                Id = id,
                PatientId = patientIdValue, // External ID from Kroll database (ARID)
                Name = reader.IsDBNull("Name") ? string.Empty : reader.GetString("Name"),
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                DefaultEmail = reader.IsDBNull("DefaultEmail") ? null : reader.GetString("DefaultEmail"),
                Status = reader.IsDBNull("Status") ? "active" : reader.GetString("Status"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.UtcNow : reader.GetDateTime("CreatedDate")
              });
            }
          }
        }
      }

      return ApplicationResult<List<PatientResponseDto>>.SuccessResult(patients, patients.Count);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching patient dropdown from Pharmacy database");
      return ApplicationResult<List<PatientResponseDto>>.Error($"Error fetching patients: {ex.Message}");
    }
  }
}

