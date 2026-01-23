using System.Data;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Core.DTOs.Base;

namespace PMS.API.Application.Features.Organizations.Queries.GetOrganizations;

public class GetOrganizationsQuery : PagedQueryBaseRequest, IRequest<ApplicationResult<List<OrganizationResponseDto>>>
{
}

public class GetOrganizationsQueryHandler : RequestHandlerBase<GetOrganizationsQuery, ApplicationResult<List<OrganizationResponseDto>>>
{
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;

  public GetOrganizationsQueryHandler(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<GetOrganizationsQueryHandler> logger) : base(serviceProvider, logger)
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

  protected override async Task<ApplicationResult<List<OrganizationResponseDto>>> HandleRequest(GetOrganizationsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var searchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword.Trim() : "";
      var hasSearch = !string.IsNullOrEmpty(searchKeyword);
      var searchPattern = hasSearch ? $"%{searchKeyword}%" : "";

      // Base query to get organizations from SQL Server Pharmacy database
      var baseQuery = $@"
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
        baseQuery += @"
          AND nh.Name LIKE @SearchKeyword";
      }

      // Count query
      var countQuery = $@"
        SELECT COUNT(DISTINCT nh.ID)
        FROM [{_databaseName}].dbo.NH nh
        WHERE 1=1";

      if (hasSearch)
      {
        countQuery += @"
          AND nh.Name LIKE @SearchKeyword";
      }

      // Pagination
      var orderBy = request.SortByAscending ? "ORDER BY nh.Name ASC" : "ORDER BY nh.Name DESC";
      var paginatedQuery = $@"
        {baseQuery}
        {orderBy}
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

      var organizations = new List<OrganizationResponseDto>();
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
              organizations.Add(new OrganizationResponseDto
              {
                Id = id,
                Name = reader.IsDBNull("Name") ? string.Empty : reader.GetString("Name"),
                WardIds = null, // WardIds are stored in PostgreSQL, not in Kroll
                Address = reader.IsDBNull("Address") ? string.Empty : reader.GetString("Address"),
                DefaultEmail = reader.IsDBNull("DefaultEmail") ? null : reader.GetString("DefaultEmail"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.UtcNow : reader.GetDateTime("CreatedDate")
              });
            }
          }
        }
      }

      return ApplicationResult<List<OrganizationResponseDto>>.SuccessResult(organizations, totalCount);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching organizations from Pharmacy database");
      return ApplicationResult<List<OrganizationResponseDto>>.Error($"Error fetching organizations: {ex.Message}");
    }
  }
}

