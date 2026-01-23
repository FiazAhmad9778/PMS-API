using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Core.DTOs.Base;
using System.Data;

namespace PMS.API.Application.Features.Organizations.Queries.GetWards;

public class GetWardsQuery : IRequest<ApplicationResult<List<WardResponseDto>>>
{
  public long OrganizationId { get; set; }
  public string? SearchKeyword { get; set; }
}

public class GetWardsQueryHandler : RequestHandlerBase<GetWardsQuery, ApplicationResult<List<WardResponseDto>>>
{
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;

  public GetWardsQueryHandler(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<GetWardsQueryHandler> logger) : base(serviceProvider, logger)
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

  protected override async Task<ApplicationResult<List<WardResponseDto>>> HandleRequest(GetWardsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var searchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword.Trim() : "";
      var hasSearch = !string.IsNullOrEmpty(searchKeyword);
      var searchPattern = hasSearch ? $"%{searchKeyword}%" : "";

      // Query to get distinct wards for a specific organization from Kroll
      var query = $@"
        SELECT DISTINCT
          nhw.ID as ExternalId,
          nhw.Name as Name
        FROM [{_databaseName}].dbo.Pat pat
        LEFT JOIN [{_databaseName}].dbo.NH nh ON nh.ID = pat.NHID
        LEFT JOIN [{_databaseName}].dbo.NHWard nhw ON nhw.ID = pat.NHWardID
        WHERE pat.NHID = @OrganizationId
          AND nhw.Name IS NOT NULL";

      if (hasSearch)
      {
        query += @"
          AND nhw.Name LIKE @SearchKeyword";
      }

      query += @"
        ORDER BY nhw.Name ASC";

      var wards = new List<WardResponseDto>();

      using (var connection = new SqlConnection(_connectionString))
      {
        await connection.OpenAsync(cancellationToken);

        using (var command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@OrganizationId", request.OrganizationId);
          if (hasSearch)
          {
            command.Parameters.AddWithValue("@SearchKeyword", searchPattern);
          }

          using (var reader = await command.ExecuteReaderAsync(cancellationToken))
          {
            while (await reader.ReadAsync(cancellationToken))
            {
              if (!reader.IsDBNull("Name"))
              {
                var externalId = reader.IsDBNull("ExternalId") ? null : reader.GetInt32("ExternalId").ToString();
                wards.Add(new WardResponseDto
                {
                  Id = 0, // Not stored in PostgreSQL yet
                  Name = reader.GetString("Name"),
                  ExternalId = externalId
                });
              }
            }
          }
        }
      }

      return ApplicationResult<List<WardResponseDto>>.SuccessResult(wards, wards.Count);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching wards from Pharmacy database");
      return ApplicationResult<List<WardResponseDto>>.Error($"Error fetching wards: {ex.Message}");
    }
  }
}

