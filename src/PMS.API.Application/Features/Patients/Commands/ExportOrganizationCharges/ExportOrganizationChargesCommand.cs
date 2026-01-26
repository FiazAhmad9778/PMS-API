using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.DTO;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace PMS.API.Application.Features.Patients.Commands.ExportOrganizationCharges;

public class ExportOrganizationChargesResponse
{
  public List<PatientFinancialResponseDto> Charges { get; set; } = new();
  public List<ClientSummaryDto> Clients { get; set; } = new();
}

public class ExportOrganizationChargesCommand : IRequest<ApplicationResult<ExportOrganizationChargesResponse>>
{
  [Required]
  public int NHID { get; set; } // Organization ID (required)

  public List<int>? WardIds { get; set; } // Optional list of Ward IDs

  [Required]
  public DateTime FromDate { get; set; }

  [Required]
  public DateTime ToDate { get; set; }
}

public class ExportOrganizationChargesCommandHandler : RequestHandlerBase<ExportOrganizationChargesCommand, ApplicationResult<ExportOrganizationChargesResponse>>
{
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;

  public ExportOrganizationChargesCommandHandler(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<ExportOrganizationChargesCommandHandler> logger) : base(serviceProvider, logger)
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

  protected override async Task<ApplicationResult<ExportOrganizationChargesResponse>> HandleRequest(ExportOrganizationChargesCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate date range
      if (request.FromDate > request.ToDate)
      {
        return ApplicationResult<ExportOrganizationChargesResponse>.Error("FromDate cannot be greater than ToDate.");
      }

      // Build Ward filter condition
      string wardFilterCondition;
      if (request.WardIds == null || !request.WardIds.Any())
      {
        // If WardIds is null or empty, fetch all wards for the organization (no ward filter)
        wardFilterCondition = "";
      }
      else
      {
        // If WardIds provided, filter by IN clause using STRING_SPLIT
        wardFilterCondition = "AND w.ID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@WardIds, ','))";
      }

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

            -- NH filter (via NHWard) - Required
            AND w.NHID = @NHID

            -- Ward filter (multiple wards support)
            {wardFilterCondition}

            -- Date range
            AND ad.[Date] >= @FromDate
            AND ad.[Date] < DATEADD(DAY, 1, @ToDate)

        ORDER BY
            ad.[Date],
            p.LastName,
            p.FirstName";

      var results = new List<PatientFinancialResponseDto>();

      // Fetch client summary data
      var clientSummaryQuery = $@"
        SELECT
            p.LastName + ', ' + p.FirstName AS PatientName,
            w.Name AS LocationHome,
            ar.AccountNum AS SeamLessCode,
            SUM(ISNULL(ai.SubTotal, 0)) AS ChargesOnAccount,
            SUM(ISNULL(ai.Tax1, 0) + ISNULL(ai.Tax2, 0)) AS TaxIncluded,
            CAST(SUM(ISNULL(ai.PaymentPending, 0)) AS NUMERIC(18,2)) AS PaymentsMade,
            SUM(ISNULL(ai.Paid, 0)) AS OutstandingCharges
        FROM [{_databaseName}].dbo.NHWard w
        INNER JOIN [{_databaseName}].dbo.Pat p ON p.NHWardID = w.ID
        INNER JOIN [{_databaseName}].dbo.AR ar ON ar.BillToPatId = p.ID
        INNER JOIN [{_databaseName}].dbo.ARInvoice ai ON ai.ARID = ar.ID
        WHERE
            w.NHID = @NHID
            {wardFilterCondition}
            AND ai.InvoiceDate >= @FromDate
            AND ai.InvoiceDate < DATEADD(DAY, 1, @ToDate)
        GROUP BY
            p.LastName,
            p.FirstName,
            w.Name,
            ar.AccountNum
        ORDER BY
            p.LastName,
            p.FirstName";

      var clientResults = new List<ClientSummaryDto>();

      using (var connection = new SqlConnection(_connectionString))
      {
        await connection.OpenAsync(cancellationToken);

        // Execute charges query
        using (var command = new SqlCommand(query, connection))
        {
          // Add NHID parameter (required)
          command.Parameters.AddWithValue("@NHID", request.NHID);

          // Add WardIds parameter
          var wardIdsParam = new SqlParameter("@WardIds", SqlDbType.NVarChar);
          if (request.WardIds != null && request.WardIds.Any())
          {
            wardIdsParam.Value = string.Join(",", request.WardIds);
          }
          else
          {
            wardIdsParam.Value = DBNull.Value;
          }
          command.Parameters.Add(wardIdsParam);

          // Add date parameters
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

        // Execute client summary query
        using (var command = new SqlCommand(clientSummaryQuery, connection))
        {
          // Add NHID parameter (required)
          command.Parameters.AddWithValue("@NHID", request.NHID);

          // Add WardIds parameter
          var wardIdsParam = new SqlParameter("@WardIds", SqlDbType.NVarChar);
          if (request.WardIds != null && request.WardIds.Any())
          {
            wardIdsParam.Value = string.Join(",", request.WardIds);
          }
          else
          {
            wardIdsParam.Value = DBNull.Value;
          }
          command.Parameters.Add(wardIdsParam);

          // Add date parameters
          command.Parameters.AddWithValue("@FromDate", request.FromDate);
          command.Parameters.AddWithValue("@ToDate", request.ToDate);

          using (var reader = await command.ExecuteReaderAsync(cancellationToken))
          {
            while (await reader.ReadAsync(cancellationToken))
            {
              clientResults.Add(new ClientSummaryDto
              {
                PatientName = reader.IsDBNull("PatientName") ? null : reader.GetString("PatientName"),
                LocationHome = reader.IsDBNull("LocationHome") ? null : reader.GetString("LocationHome"),
                SeamLessCode = reader.IsDBNull("SeamLessCode") ? null : reader.GetString("SeamLessCode"),
                ChargesOnAccount = reader.IsDBNull("ChargesOnAccount") ? 0 : reader.GetDecimal("ChargesOnAccount"),
                TaxIncluded = reader.IsDBNull("TaxIncluded") ? 0 : reader.GetDecimal("TaxIncluded"),
                PaymentsMade = reader.IsDBNull("PaymentsMade") ? 0 : reader.GetDecimal("PaymentsMade"),
                OutstandingCharges = reader.IsDBNull("OutstandingCharges") ? 0 : reader.GetDecimal("OutstandingCharges")
              });
            }
          }
        }
      }

      var response = new ExportOrganizationChargesResponse
      {
        Charges = results,
        Clients = clientResults
      };

      return ApplicationResult<ExportOrganizationChargesResponse>.SuccessResult(response, results.Count + clientResults.Count);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching organization charges from Pharmacy database. NHID: {NHID}, WardIds: {WardIds}", 
        request.NHID, request.WardIds != null ? string.Join(",", request.WardIds) : "null");
      return ApplicationResult<ExportOrganizationChargesResponse>.Error($"Error fetching organization charges: {ex.Message}");
    }
  }
}
