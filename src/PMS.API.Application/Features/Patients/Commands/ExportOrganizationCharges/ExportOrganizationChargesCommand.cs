using Dapper;
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
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Infrastructure.Data;
using PMS.API.Infrastructure.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace PMS.API.Application.Features.Patients.Commands.ExportOrganizationCharges;

public class ExportOrganizationChargesResponse
{
  public List<PatientFinancialResponseDto> Charges { get; set; } = new();
  public List<ClientSummaryDto> Clients { get; set; } = new();
}

public class ExportOrganizationChargesCommand
    : IRequest<ApplicationResult<bool>>
{
  public List<long>? OrganizationIds { get; set; }
  public List<long>? PatientIds { get; set; }

  [Required]
  public DateTime FromDate { get; set; }

  [Required]
  public DateTime ToDate { get; set; }

  public bool IsSent { get; set; }
  public int[]? InvoiceSendingWays { get; set; }
}


public class ExportOrganizationChargesCommandHandler : RequestHandlerBase<ExportOrganizationChargesCommand, ApplicationResult<bool>>
{
  readonly IConfiguration _configuration;
  readonly string _connectionString;
  readonly string _databaseName;
  readonly IBackgroundTaskQueue _backgroundTaskQueue;
  readonly IServiceScopeFactory _scopeFactory;
  readonly AppDbContext _appDbContext;
  readonly ICurrentUserService _currentUserService;
  readonly IRepository<Ward> _wardRepo;

  public ExportOrganizationChargesCommandHandler(
   IConfiguration configuration,
   IServiceProvider serviceProvider,
   ILogger<ExportOrganizationChargesCommandHandler> logger,
   IBackgroundTaskQueue backgroundTaskQueue,
   IServiceScopeFactory scopeFactory,
   AppDbContext appDbContext,
   ICurrentUserService currentUserService,
   IRepository<Ward> wardRepo)
   : base(serviceProvider, logger)
  {
    _configuration = configuration;
    _backgroundTaskQueue = backgroundTaskQueue;
    _scopeFactory = scopeFactory;
    _appDbContext = appDbContext;
    _currentUserService = currentUserService;
    _wardRepo = wardRepo;

    _connectionString = _configuration.GetConnectionString("ARDashboardConnection")
      ?? throw new InvalidOperationException("Connection string 'ARDashboardConnection' not found.");

    _databaseName = ConnectionStringHelper.ExtractDatabaseName(_connectionString);
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(
     ExportOrganizationChargesCommand request,
     CancellationToken cancellationToken)
  {
    if (request.FromDate > request.ToDate)
      return ApplicationResult<bool>.Error("FromDate cannot be greater than ToDate.");

    if ((request.OrganizationIds == null || !request.OrganizationIds.Any()) &&
        (request.PatientIds == null || !request.PatientIds.Any()))
      return ApplicationResult<bool>.Error("OrganizationIds or PatientIds required.");

    if (request.OrganizationIds?.Any() == true)
    {
      foreach (var organizationId in request.OrganizationIds)
      {
        await ProcessOrganizationInvoice(
            organizationId, request, cancellationToken);
      }
    }
    else
    {
      foreach (var patientId in request.PatientIds!)
      {
        await ProcessPatientInvoice(
            patientId, request, cancellationToken);
      }
    }

    return ApplicationResult<bool>.SuccessResult(true);
  }

  private async Task ProcessOrganizationInvoice(
    long organizationId,
    ExportOrganizationChargesCommand request,
    CancellationToken ct)
  {
    var externalOrganizationId = await _appDbContext.Organization.Where(x => x.OrganizationExternalId == organizationId).Select(x => x.Id).FirstOrDefaultAsync();
    var wardIds = await _appDbContext.Ward
        .Where(w => w.OrganizationId == externalOrganizationId)
        .Select(w => w.ExternalId)
        .ToListAsync(ct);

    if (!wardIds.Any())
      return;

    var (charges, clients) =
        await FetchChargesByWardIds(wardIds, request, ct);

    if (!charges.Any())
      return;

    var exists = await _appDbContext.PatientInvoiceHistory
        .AnyAsync(h =>
            h.OrganizationId == organizationId &&
            h.InvoiceStartDate <= request.ToDate &&
            h.InvoiceEndDate >= request.FromDate,
            ct);

    if (exists)
      throw new InvalidOperationException("Invoice already exists.");

    var filePath = SaveExcelFile(
        charges, clients, $"ORG_{organizationId}", request);

    await CreateInvoiceHistoryForOrganization(
        organizationId,
        wardIds,
        charges,
        filePath,
        request,
        ct);
  }


  private async Task ProcessPatientInvoice(
    long patientId,
    ExportOrganizationChargesCommand request,
    CancellationToken ct)
  {
    var (charges, clients) =
        await FetchChargesByPatientId(patientId, request, ct);

    if (!charges.Any())
      return;

    var filePath = SaveExcelFile(
        charges, clients, $"PAT_{patientId}", request);

    var history = new PatientInvoiceHistory
    {
      PatientId = patientId,
      InvoiceStartDate = request.FromDate,
      InvoiceEndDate = request.ToDate,
      FilePath = filePath,
      IsSent = request.IsSent,
      InvoiceSendingWays = request.InvoiceSendingWays != null
            ? string.Join(",", request.InvoiceSendingWays)
            : null,
      CreatedBy = _currentUserService.UserId,
      CreatedDate = DateTime.UtcNow
    };

    _appDbContext.PatientInvoiceHistory.Add(history);
    await _appDbContext.SaveChangesAsync(ct);
  }



  private async Task<(List<PatientFinancialResponseDto>, List<ClientSummaryDto>)>
 FetchChargesByWardIds(
     List<long> wardIds,
     ExportOrganizationChargesCommand request,
     CancellationToken ct)
  {
    if (wardIds == null || !wardIds.Any())
      return (new(), new());

    var wardIdsCsv = string.Join(",", wardIds);

    var chargesQuery = $@"
        SELECT
            p.ID AS PatientId,
            w.ID AS WardId,
            ad.[Date],
            p.LastName + ', ' + p.FirstName AS PatientName,
            ar.AccountNum AS SeamCode,
            ad.Comment AS ChargeDescription,
            ad.Amount
        FROM [{_databaseName}].dbo.NHWard w
        JOIN [{_databaseName}].dbo.Pat p ON p.NHWardID = w.ID
        JOIN [{_databaseName}].dbo.AR ar ON ar.BillToPatID = p.ID
        JOIN [{_databaseName}].dbo.ARDetail ad ON ad.ARID = ar.ID
        WHERE
            w.ID IN (
                SELECT CAST(value AS BIGINT)
                FROM STRING_SPLIT(@WardIds, ',')
            )
            AND ad.[Date] >= @FromDate
            AND ad.[Date] < DATEADD(DAY, 1, @ToDate)
            AND EXISTS (
                SELECT 1
                FROM [{_databaseName}].dbo.ARPayment ap
                WHERE ap.ARID = ar.ID
                  AND ap.Status IN (1, 2)
            );";

    var clientSummaryQuery = $@"
        SELECT
            w.ID AS WardId,
            w.Name AS LocationHome,
            COUNT(DISTINCT p.ID) AS PatientCount,
            SUM(ISNULL(ai.SubTotal, 0)) AS ChargesOnAccount,
            SUM(ISNULL(ai.Tax1, 0) + ISNULL(ai.Tax2, 0)) AS TaxIncluded,
            SUM(ISNULL(ai.PaymentPending, 0)) AS PaymentsMade,
            SUM(ISNULL(ai.Paid, 0)) AS OutstandingCharges
        FROM [{_databaseName}].dbo.NHWard w
        JOIN [{_databaseName}].dbo.Pat p ON p.NHWardID = w.ID
        JOIN [{_databaseName}].dbo.AR ar ON ar.BillToPatID = p.ID
        JOIN [{_databaseName}].dbo.ARInvoice ai ON ai.ARID = ar.ID
        WHERE
            w.ID IN (
                SELECT CAST(value AS BIGINT)
                FROM STRING_SPLIT(@WardIds, ',')
            )
            AND ai.InvoiceDate >= @FromDate
            AND ai.InvoiceDate < DATEADD(DAY, 1, @ToDate)
        GROUP BY
            w.ID, w.Name;";

    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync(ct);

    var charges = (await connection.QueryAsync<PatientFinancialResponseDto>(
        chargesQuery,
        new
        {
          WardIds = wardIdsCsv,
          request.FromDate,
          request.ToDate
        })).ToList();

    var clients = (await connection.QueryAsync<ClientSummaryDto>(
        clientSummaryQuery,
        new
        {
          WardIds = wardIdsCsv,
          request.FromDate,
          request.ToDate
        })).ToList();

    return (charges, clients);
  }


  private string SaveExcelFile(
      List<PatientFinancialResponseDto> charges,
      List<ClientSummaryDto> clients,
      string prefix,
      ExportOrganizationChargesCommand request)
  {
    var folder = Path.Combine("wwwroot", "Invoices");
    Directory.CreateDirectory(folder);

    var fileName =
        $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

    var fullPath = Path.Combine(folder, fileName);

    var bytes = ExcelExportHelper.GenerateOrganizationChargesExcel(
        charges, clients, request.FromDate, request.ToDate, folder);

    File.WriteAllBytes(fullPath, bytes);

    return $"Invoices/{fileName}";
  }
  private async Task CreateInvoiceHistoryForOrganization(
      long organizationId,
      List<long> wardIds,
      List<PatientFinancialResponseDto> charges,
      string filePath,
      ExportOrganizationChargesCommand request,
      CancellationToken ct)
  {
    var history = new PatientInvoiceHistory
    {
      OrganizationId = organizationId,
      InvoiceStartDate = request.FromDate,
      InvoiceEndDate = request.ToDate,
      FilePath = filePath,
      IsSent = request.IsSent,
      InvoiceSendingWays = request.InvoiceSendingWays != null
            ? string.Join(",", request.InvoiceSendingWays)
            : null,
      CreatedBy = _currentUserService.UserId,
      PatientInvoiceHistoryWardList = charges
            .GroupBy(x => x.WardId)
            .Select(g => new PatientInvoiceHistoryWard
            {
              WardId = g.Key,
              PatientIds = string.Join(",", g.Select(x => x.PatientId).Distinct())
            }).ToList()
    };

    _appDbContext.PatientInvoiceHistory.Add(history);
    await _appDbContext.SaveChangesAsync(ct);
  }

  private async Task<(List<PatientFinancialResponseDto>, List<ClientSummaryDto>)>
FetchChargesByPatientId(
    long patientId,
    ExportOrganizationChargesCommand request,
    CancellationToken ct)
  {
    var chargesQuery = $@"
        SELECT
            p.ID AS PatientId,
            w.ID AS WardId,
            ad.[Date],
            p.LastName + ', ' + p.FirstName AS PatientName,
            ar.AccountNum AS SeamCode,
            ad.Comment AS ChargeDescription,
            ad.Amount
        FROM [{_databaseName}].dbo.Pat p
        LEFT JOIN [{_databaseName}].dbo.NHWard w ON w.ID = p.NHWardID
        JOIN [{_databaseName}].dbo.AR ar ON ar.BillToPatID = p.ID
        JOIN [{_databaseName}].dbo.ARDetail ad ON ad.ARID = ar.ID
        WHERE
            p.ID = @PatientId
            AND ad.[Date] >= @FromDate
            AND ad.[Date] < DATEADD(DAY, 1, @ToDate)
            AND EXISTS (
                SELECT 1
                FROM [{_databaseName}].dbo.ARPayment ap
                WHERE ap.ARID = ar.ID
                  AND ap.Status IN (1, 2)
            );";

    var clientSummaryQuery = $@"
        SELECT
            p.ID AS PatientId,
            ISNULL(w.ID, 0) AS WardId,
            ISNULL(w.Name, 'N/A') AS LocationHome,
            COUNT(DISTINCT ar.ID) AS InvoiceCount,
            SUM(ISNULL(ai.SubTotal, 0)) AS ChargesOnAccount,
            SUM(ISNULL(ai.Tax1, 0) + ISNULL(ai.Tax2, 0)) AS TaxIncluded,
            SUM(ISNULL(ai.PaymentPending, 0)) AS PaymentsMade,
            SUM(ISNULL(ai.Paid, 0)) AS OutstandingCharges
        FROM [{_databaseName}].dbo.Pat p
        LEFT JOIN [{_databaseName}].dbo.NHWard w ON w.ID = p.NHWardID
        JOIN [{_databaseName}].dbo.AR ar ON ar.BillToPatID = p.ID
        JOIN [{_databaseName}].dbo.ARInvoice ai ON ai.ARID = ar.ID
        WHERE
            p.ID = @PatientId
            AND ai.InvoiceDate >= @FromDate
            AND ai.InvoiceDate < DATEADD(DAY, 1, @ToDate)
        GROUP BY
            p.ID, w.ID, w.Name;";

    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync(ct);

    var charges = (await connection.QueryAsync<PatientFinancialResponseDto>(
        chargesQuery,
        new
        {
          PatientId = patientId,
          request.FromDate,
          request.ToDate
        })).ToList();

    var clientSummary = (await connection.QueryAsync<ClientSummaryDto>(
        clientSummaryQuery,
        new
        {
          PatientId = patientId,
          request.FromDate,
          request.ToDate
        })).ToList();

    return (charges, clientSummary);
  }


}
