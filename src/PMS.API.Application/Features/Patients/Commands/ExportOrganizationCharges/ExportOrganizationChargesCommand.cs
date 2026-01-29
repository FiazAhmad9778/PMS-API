using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.BackgroundWorker;
using PMS.API.Application.Common.ConectionStringHelper;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;
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
  public int NHID { get; set; }

  public List<int>? WardIds { get; set; }

  [Required]
  public DateTime FromDate { get; set; }

  [Required]
  public DateTime ToDate { get; set; }
  public bool IsSent { get; set; } = false;
  public int[]? InvoiceSendingWays { get; set; }
}

public class ExportOrganizationChargesCommandHandler : RequestHandlerBase<ExportOrganizationChargesCommand, ApplicationResult<ExportOrganizationChargesResponse>>
{
  readonly IConfiguration _configuration;
  readonly string _connectionString;
  readonly string _databaseName;
  readonly IBackgroundTaskQueue _backgroundTaskQueue;
  readonly IServiceScopeFactory _scopeFactory;
  readonly AppDbContext _appDbContext;

  public ExportOrganizationChargesCommandHandler(
   IConfiguration configuration,
   IServiceProvider serviceProvider,
   ILogger<ExportOrganizationChargesCommandHandler> logger,
   IBackgroundTaskQueue backgroundTaskQueue,
   IServiceScopeFactory scopeFactory,
   AppDbContext appDbContext)
   : base(serviceProvider, logger)
  {
    _configuration = configuration;
    _backgroundTaskQueue = backgroundTaskQueue;
    _scopeFactory = scopeFactory;
    _appDbContext = appDbContext;

    _connectionString = _configuration.GetConnectionString("ARDashboardConnection")
      ?? throw new InvalidOperationException("Connection string 'ARDashboardConnection' not found.");

    _databaseName = ConnectionStringHelper.ExtractDatabaseName(_connectionString);
  }

  protected override async Task<ApplicationResult<ExportOrganizationChargesResponse>> HandleRequest(ExportOrganizationChargesCommand request, CancellationToken cancellationToken)
  {
    try
    {
      if (request.FromDate > request.ToDate)
      {
        return ApplicationResult<ExportOrganizationChargesResponse>.Error("FromDate cannot be greater than ToDate.");
      }

      string wardFilterCondition;
      if (request.WardIds == null || !request.WardIds.Any())
      {
        wardFilterCondition = "";
      }
      else
      {
        wardFilterCondition = "AND w.ID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@WardIds, ','))";
      }

      var query = $@"
        SELECT
          p.ID AS PatientId,
          w.ID AS WardId,

          ad.[Date] AS [Date],
          p.LastName + ', ' + p.FirstName AS PatientName,
          p.Address1 AS [Your Code],
          ar.AccountNum AS [Seam Code],
          ad.Comment AS ChargeDescription,
          CASE ad.TaxType
              WHEN 0 THEN 'Exempt'
              WHEN 4 THEN 'HST'
              ELSE 'Other'
          END AS TaxType,
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

      var clientSummaryQuery = $@"
        SELECT
            p.Id AS PatientId,
            p.LastName + ', ' + p.FirstName AS PatientName,
            w.Id as WardId,
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
            p.Id,        
            p.LastName,
            p.FirstName,
            w.Id,        
            w.Name,
            ar.AccountNum

        ORDER BY
            p.LastName,
            p.FirstName";

      var clientResults = new List<ClientSummaryDto>();

      using (var connection = new SqlConnection(_connectionString))
      {
        await connection.OpenAsync(cancellationToken);

        using (var command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@NHID", request.NHID);

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

          command.Parameters.AddWithValue("@FromDate", request.FromDate);
          command.Parameters.AddWithValue("@ToDate", request.ToDate);

          using (var reader = await command.ExecuteReaderAsync(cancellationToken))
          {
            while (await reader.ReadAsync(cancellationToken))
            {
              results.Add(new PatientFinancialResponseDto
              {
                PatientId = reader.IsDBNull("PatientId") ? 0 : reader.GetInt32("PatientId"),
                WardId = reader.IsDBNull("WardId") ? 0 : reader.GetInt32("WardId"),
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

        var exists = await IsInvoiceExist(request, results);

        if (exists)
          return ApplicationResult<ExportOrganizationChargesResponse>.Error("Invoice already exists for the selected date range.");

        using (var command = new SqlCommand(clientSummaryQuery, connection))
        {
          command.Parameters.AddWithValue("@NHID", request.NHID);

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

          command.Parameters.AddWithValue("@FromDate", request.FromDate);
          command.Parameters.AddWithValue("@ToDate", request.ToDate);

          using (var reader = await command.ExecuteReaderAsync(cancellationToken))
          {
            while (await reader.ReadAsync(cancellationToken))
            {
              clientResults.Add(new ClientSummaryDto
              {
                PatientId = reader.IsDBNull("PatientId") ? 0 : reader.GetInt32("PatientId"),
                PatientName = reader.IsDBNull("PatientName") ? null : reader.GetString("PatientName"),
                WardId = reader.IsDBNull("WardId") ? 0 : reader.GetInt32("WardId"),
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

      if (results.Any())
        CreateInvoiceHistory(request, results);

      return ApplicationResult<ExportOrganizationChargesResponse>.SuccessResult(response!, results.Count + clientResults.Count);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching organization charges from Pharmacy database. NHID: {NHID}, WardIds: {WardIds}",
       request.NHID, request.WardIds != null ? string.Join(",", request.WardIds) : "null");
      return ApplicationResult<ExportOrganizationChargesResponse>.Error($"Error fetching organization charges: {ex.Message}");
    }
  }

  private async Task<bool> IsInvoiceExist(ExportOrganizationChargesCommand request, List<PatientFinancialResponseDto> results)
  {
    var wardIds = results.Select(r => r.WardId).Distinct().ToList();

    var exists = await _appDbContext.PatientInvoiceHistory
        .AnyAsync(h =>
            h.OrganizationId == request.NHID &&

            // Date range overlap check
            h.InvoiceStartDate <= request.ToDate &&
            h.InvoiceEndDate >= request.FromDate &&

            h.PatientInvoiceHistoryWardList
                .Any(w => wardIds.Contains(w.WardId))
        );
    return exists;
  }

  private void CreateInvoiceHistory(ExportOrganizationChargesCommand request, List<PatientFinancialResponseDto> results)
  {
    _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
    {
      using var scope = _scopeFactory.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();



      var history = new PatientInvoiceHistory
      {
        OrganizationId = request.NHID,
        InvoiceStartDate = request.FromDate,
        InvoiceEndDate = request.ToDate,
        FilePath = string.Empty,
        IsSent = request.IsSent,
        InvoiceSendingWays = request.InvoiceSendingWays != null && request.InvoiceSendingWays.Any() ?
                              string.Join(",", request.InvoiceSendingWays) : string.Empty,
        CreatedBy = 1,
        PatientInvoiceHistoryWardList = results
              .GroupBy(r => r.WardId)
              .Select(g => new PatientInvoiceHistoryWard
              {
                WardId = g.Key,
                PatientIds = string.Join(",", g.Select(x => x.PatientId).Distinct())
              })
              .ToList()
      };

      db.PatientInvoiceHistory.Add(history);
      await db.SaveChangesAsync(ct);
    });
  }
}
