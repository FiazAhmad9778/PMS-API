using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Core.DTOs.Base;
using System.Data;

namespace PMS.API.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsQuery : PagedQueryBaseRequest, IRequest<ApplicationResult<List<PatientResponseDto>>>
{
}

public class GetPatientsQueryHandler : RequestHandlerBase<GetPatientsQuery, ApplicationResult<List<PatientResponseDto>>>
{
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;

  public GetPatientsQueryHandler(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<GetPatientsQueryHandler> logger) : base(serviceProvider, logger)
  {
    _configuration = configuration;
    // Use ARDashboardConnection for SQL Server (Pharmacy database is on SQL Server)
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

  protected override async Task<ApplicationResult<List<PatientResponseDto>>> HandleRequest(GetPatientsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var searchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword.Trim() : "";
      var hasSearch = !string.IsNullOrEmpty(searchKeyword);
      var searchPattern = hasSearch ? $"%{searchKeyword}%" : "";

      // Base query to get distinct patients
      var baseQuery = $@"
        SELECT DISTINCT
          pat.ARID as Id,
          pat.ARID as PatientId,
          CONCAT(pat.LastName, ', ', pat.FirstName) as Name,
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

      // Add search condition if needed - only search by patient name
      if (hasSearch)
      {
        baseQuery += @"
          AND CONCAT(pat.LastName, ', ', pat.FirstName) LIKE @SearchKeyword";
      }

      // Count query
      var countQuery = $@"
        SELECT COUNT(DISTINCT pat.ARID)
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

      if (hasSearch)
      {
        countQuery += @"
          AND CONCAT(pat.LastName, ', ', pat.FirstName) LIKE @SearchKeyword";
      }

      // Pagination
      var orderBy = request.SortByAscending ? "ORDER BY pat.LastUsed ASC" : "ORDER BY pat.LastUsed DESC";
      var paginatedQuery = $@"
        {baseQuery}
        {orderBy}
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

      var patients = new List<PatientResponseDto>();
      int totalCount = 0;

      using (var connection = new SqlConnection(_connectionString))
      {
        await connection.OpenAsync(cancellationToken);

        // Get total count
        using (var countCommand = new SqlCommand(countQuery, connection))
        {
          if (hasSearch)
          {
            countCommand.Parameters.AddWithValue("@SearchKeyword", searchPattern);
          }

          var countResult = await countCommand.ExecuteScalarAsync(cancellationToken);
          totalCount = countResult != DBNull.Value ? Convert.ToInt32(countResult) : 0;
        }

        // Get paginated results
        using (var command = new SqlCommand(paginatedQuery, connection))
        {
          if (hasSearch)
          {
            command.Parameters.AddWithValue("@SearchKeyword", searchPattern);
          }
          command.Parameters.AddWithValue("@Offset", (request.PageNumber - 1) * request.PageSize);
          command.Parameters.AddWithValue("@PageSize", request.PageSize);

          using (var reader = await command.ExecuteReaderAsync(cancellationToken))
          {
            while (await reader.ReadAsync(cancellationToken))
            {
              var id = reader.IsDBNull("Id") ? 0 : reader.GetInt32("Id");
              var patientIdValue = reader.IsDBNull("PatientId") ? id : reader.GetInt32("PatientId");
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

      return ApplicationResult<List<PatientResponseDto>>.SuccessResult(patients, totalCount);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching patients from Pharmacy database");
      return ApplicationResult<List<PatientResponseDto>>.Error($"Error fetching patients: {ex.Message}");
    }
  }
}

