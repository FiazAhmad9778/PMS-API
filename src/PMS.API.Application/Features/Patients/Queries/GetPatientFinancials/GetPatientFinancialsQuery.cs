using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.DTO;
using System.Data;

namespace PMS.API.Application.Features.Patients.Queries.GetPatientFinancials;

public class GetPatientFinancialsQuery : IRequest<ApplicationResult<List<PatientFinancialResponseDto>>>
{
  public int? PatientId { get; set; }
  public int? NHID { get; set; } // Organization ID
  public int? WardID { get; set; }
  public DateTime FromDate { get; set; }
  public DateTime ToDate { get; set; }
}

public class GetPatientFinancialsQueryHandler : RequestHandlerBase<GetPatientFinancialsQuery, ApplicationResult<List<PatientFinancialResponseDto>>>
{
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;

  public GetPatientFinancialsQueryHandler(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<GetPatientFinancialsQueryHandler> logger) : base(serviceProvider, logger)
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

  protected override async Task<ApplicationResult<List<PatientFinancialResponseDto>>> HandleRequest(GetPatientFinancialsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var query = $@"
        SELECT
          -- Date
          ad.[Date] AS [Date],

          -- Patient Name
          p.LastName + ', ' + p.FirstName AS PatientName,

          -- Your Code
          p.Address1 AS [Your Code],

          -- Seam Code
          ar.AccountNum AS [Seam Code],

          -- Charge Description
          ad.Comment AS ChargeDescription,

          -- Tax Type
          CASE ad.TaxType
              WHEN 0 THEN 'Exempt'
              WHEN 4 THEN 'HST'
              ELSE 'Other'
          END AS TaxType,

          -- Amount
          ad.Amount AS Amount

        FROM [{_databaseName}].dbo.NHWard w
        JOIN [{_databaseName}].dbo.Pat p
            ON p.NHWardID = w.ID
        JOIN [{_databaseName}].dbo.AR ar
            ON ar.BillToPatID = p.ID
        JOIN [{_databaseName}].dbo.ARDetail ad
            ON ad.ARID = ar.ID
           AND ad.PatID = p.ID

        WHERE
            -- Paid / Posted ARs only
            EXISTS (
                SELECT 1
                FROM [{_databaseName}].dbo.ARPayment ap
                WHERE ap.ARID = ar.ID
                  AND ap.Status IN (1, 2)
            )

            -- NH filter (via NHWard)
            AND (
                @NHID IS NULL
                OR w.NHID = @NHID
            )

            -- Ward filter
            AND (
                @WardID IS NULL
                OR w.ID = @WardID
            )

            -- Patient filter
            AND (
                @PatientID IS NULL
                OR p.ID = @PatientID
            )

            -- Date range
            AND ad.[Date] >= @FromDate
            AND ad.[Date] < DATEADD(DAY, 1, @ToDate)

        ORDER BY
            ad.[Date],
            p.LastName,
            p.FirstName";

      var results = new List<PatientFinancialResponseDto>();

      using (var connection = new SqlConnection(_connectionString))
      {
        await connection.OpenAsync(cancellationToken);

        using (var command = new SqlCommand(query, connection))
        {
          // Add parameters - use proper NULL handling for optional filters
          var nhidParam = new SqlParameter("@NHID", System.Data.SqlDbType.Int);
          nhidParam.Value = request.NHID.HasValue ? (object)request.NHID.Value : DBNull.Value;
          command.Parameters.Add(nhidParam);

          var wardIdParam = new SqlParameter("@WardID", System.Data.SqlDbType.Int);
          wardIdParam.Value = request.WardID.HasValue ? (object)request.WardID.Value : DBNull.Value;
          command.Parameters.Add(wardIdParam);

          var patientIdParam = new SqlParameter("@PatientID", System.Data.SqlDbType.Int);
          patientIdParam.Value = request.PatientId.HasValue ? (object)request.PatientId.Value : DBNull.Value;
          command.Parameters.Add(patientIdParam);

          command.Parameters.AddWithValue("@FromDate", request.FromDate);
          command.Parameters.AddWithValue("@ToDate", request.ToDate);

          using (var reader = await command.ExecuteReaderAsync(cancellationToken))
          {
            while (await reader.ReadAsync(cancellationToken))
            {
              results.Add(new PatientFinancialResponseDto
              {
                Date = reader.IsDBNull("Date") ? DateTime.MinValue : reader.GetDateTime("Date"),
                PatientName = reader.IsDBNull("PatientName") ? string.Empty : reader.GetString("PatientName"),
                YourCode = reader.IsDBNull("Your Code") ? null : reader.GetString("Your Code"),
                SeamCode = reader.IsDBNull("Seam Code") ? null : reader.GetString("Seam Code"),
                ChargeDescription = reader.IsDBNull("ChargeDescription") ? null : reader.GetString("ChargeDescription"),
                TaxType = reader.IsDBNull("TaxType") ? string.Empty : reader.GetString("TaxType"),
                Amount = reader.IsDBNull("Amount") ? 0 : reader.GetDecimal("Amount")
              });
            }
          }
        }
      }

      return ApplicationResult<List<PatientFinancialResponseDto>>.SuccessResult(results, results.Count);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching patient financials from Pharmacy database");
      return ApplicationResult<List<PatientFinancialResponseDto>>.Error($"Error fetching patient financials: {ex.Message}");
    }
  }
}
