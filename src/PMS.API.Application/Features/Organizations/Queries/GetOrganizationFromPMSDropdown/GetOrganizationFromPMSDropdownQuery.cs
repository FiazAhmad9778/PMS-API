using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using System.Data;

namespace PMS.API.Application.Features.Organizations.Queries.GetOrganizationDropdown;

public class GetOrganizationFromPMSDropdownQuery : IRequest<ApplicationResult<List<OrganizationResponseDto>>>
{
  public string? SearchKeyword { get; set; }
}

public class GetOrganizationFromPMSDropdownQueryHandler : RequestHandlerBase<GetOrganizationFromPMSDropdownQuery, ApplicationResult<List<OrganizationResponseDto>>>
{
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;

  public GetOrganizationFromPMSDropdownQueryHandler(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<GetOrganizationFromPMSDropdownQueryHandler> logger) : base(serviceProvider, logger)
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

  protected override async Task<ApplicationResult<List<OrganizationResponseDto>>> HandleRequest(GetOrganizationFromPMSDropdownQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var searchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword.Trim() : "";
      var hasSearch = !string.IsNullOrEmpty(searchKeyword);
      var searchPattern = hasSearch ? $"%{searchKeyword}%" : "";

      // Query to get organizations from SQL Server Pharmacy database (no pagination for dropdown, but returns full data)
      var query = $@"
        SELECT DISTINCT
          nh.ID as Id,
          nh.Name as Name,
          NULL as Address,
          NULL as DefaultEmail,
          GETDATE() as CreatedDate
        FROM [{_databaseName}].dbo.NH nh
        WHERE 1=1";

      // Add search condition if needed
      if (hasSearch)
      {
        query += @"
          AND nh.Name LIKE @SearchKeyword";
      }

      query += @"
        ORDER BY nh.Name ASC";

      var organizations = new List<OrganizationResponseDto>();

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
              organizations.Add(new OrganizationResponseDto
              {
                Id = id,
                Name = reader.IsDBNull("Name") ? string.Empty : reader.GetString("Name"),
                OrganizationExternalId = id,
                WardIds = null, // WardIds are stored in PostgreSQL, not in Kroll
                Address = reader.IsDBNull("Address") ? string.Empty : reader.GetString("Address"),
                DefaultEmail = reader.IsDBNull("DefaultEmail") ? null : reader.GetString("DefaultEmail"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.UtcNow : reader.GetDateTime("CreatedDate")
              });
            }
          }
        }
      }

      return ApplicationResult<List<OrganizationResponseDto>>.SuccessResult(organizations, organizations.Count);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching organization dropdown from Pharmacy database");
      return ApplicationResult<List<OrganizationResponseDto>>.Error($"Error fetching organizations: {ex.Message}");
    }
  }
}

