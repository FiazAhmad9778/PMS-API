using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PMS.API.Application.Common.ConectionStringHelper;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Options;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Services.Implementation;

public class InvoiceFileGenerationService : IInvoiceFileGenerationService
{
  private readonly IConfiguration _configuration;
  private readonly string _connectionString;
  private readonly string _databaseName;
  private readonly AppDbContext _appDbContext;
  private readonly StatementFromOptions _statementFromOptions;

  public InvoiceFileGenerationService(
    IConfiguration configuration,
    AppDbContext appDbContext,
    IOptions<StatementFromOptions> statementFromOptions)
  {
    _configuration = configuration;
    _appDbContext = appDbContext;
    _statementFromOptions = statementFromOptions?.Value ?? new StatementFromOptions();

    _connectionString = _configuration.GetConnectionString("ARDashboardConnection")
      ?? throw new InvalidOperationException("Connection string 'ARDashboardConnection' not found.");
    _databaseName = ConnectionStringHelper.ExtractDatabaseName(_connectionString);
  }

  public async Task<(string? FilePath, List<PatientFinancialResponseDto>? Charges)> GenerateOrganizationInvoiceAsync(
    long organizationId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default)
  {
    var externalOrganizationId = await _appDbContext.Organization
      .Where(x => x.OrganizationExternalId == organizationId)
      .Select(x => x.Id)
      .FirstOrDefaultAsync(cancellationToken);

    var wardIds = await _appDbContext.Ward
      .Where(w => w.OrganizationId == externalOrganizationId)
      .Select(w => w.ExternalId)
      .ToListAsync(cancellationToken);

    if (wardIds.Count == 0)
      return (null, null);

    var (charges, clients) = await FetchChargesByWardIdsAsync(wardIds, fromDate, toDate, cancellationToken);
    if (charges.Count == 0)
      return (null, null);

    var organization = await _appDbContext.Organization
      .Where(x => x.OrganizationExternalId == organizationId)
      .Select(x => new { x.Name, x.Address })
      .FirstOrDefaultAsync(cancellationToken);

    var statementData = BuildStatementSheetDto(
      organization?.Name ?? string.Empty,
      organization?.Address,
      clients,
      fromDate,
      toDate);

    var filePath = SaveExcelFile(charges, clients, $"ORG_{organizationId}", fromDate, toDate, statementData);
    return (filePath, charges);
  }

  public async Task<string?> GeneratePatientInvoiceAsync(
    long patientId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default)
  {
    var (charges, clients) = await FetchChargesByPatientIdAsync(patientId, fromDate, toDate, cancellationToken);
    if (charges.Count == 0)
      return null;

    var patient = await _appDbContext.Patient
      .Where(x => x.PatientId == patientId)
      .Select(x => new { x.Name, x.Address })
      .FirstOrDefaultAsync(cancellationToken);

    var statementData = BuildStatementSheetDto(
      patient?.Name ?? string.Empty,
      patient?.Address,
      clients,
      fromDate,
      toDate);

    return SaveExcelFile(charges, clients, $"PAT_{patientId}", fromDate, toDate, statementData);
  }

  private async Task<(List<PatientFinancialResponseDto>, List<ClientSummaryDto>)> FetchChargesByWardIdsAsync(
    List<long> wardIds,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken ct)
  {
    if (wardIds == null || wardIds.Count == 0)
      return (new List<PatientFinancialResponseDto>(), new List<ClientSummaryDto>());

    var wardIdsCsv = string.Join(",", wardIds);

    var chargesQuery = $@"
        SELECT
            p.ID AS PatientId,
            w.ID AS WardId,
            ad.[Date],
            p.LastName + ', ' + p.FirstName AS PatientName,
            w.Name AS YourCode,
            ar.AccountNum AS SeamCode,
            ad.Comment AS ChargeDescription,
            CASE WHEN ad.TaxType = 0 THEN 'Exempt' WHEN ad.TaxType = 4 THEN 'HST' ELSE 'Other' END AS TaxType,
            ad.Amount
        FROM [{_databaseName}].dbo.NHWard w
        JOIN [{_databaseName}].dbo.Pat p ON p.NHWardID = w.ID
        JOIN [{_databaseName}].dbo.AR ar ON ar.BillToPatID = p.ID
        JOIN [{_databaseName}].dbo.ARDetail ad ON ad.ARID = ar.ID
        WHERE
            w.ID IN (SELECT CAST(value AS BIGINT) FROM STRING_SPLIT(@WardIds, ','))
            AND ad.[Date] >= @FromDate
            AND ad.[Date] < DATEADD(DAY, 1, @ToDate)
            AND EXISTS (SELECT 1 FROM [{_databaseName}].dbo.ARPayment ap WHERE ap.ARID = ar.ID AND ap.Status IN (1, 2));";

    var clientSummaryQuery = $@"
        SELECT
            p.ID AS PatientId,
            p.LastName + ', ' + p.FirstName AS PatientName,
            w.ID AS WardId,
            w.Name AS LocationHome,
            MAX(ar.AccountNum) AS SeamLessCode,
            SUM(ISNULL(ai.SubTotal, 0)) AS ChargesOnAccount,
            SUM(ISNULL(ai.Tax1, 0) + ISNULL(ai.Tax2, 0)) AS TaxIncluded,
            SUM(ISNULL(ai.PaymentPending, 0)) AS PaymentsMade,
            SUM(ISNULL(ai.Paid, 0)) AS OutstandingCharges
        FROM [{_databaseName}].dbo.NHWard w
        JOIN [{_databaseName}].dbo.Pat p ON p.NHWardID = w.ID
        JOIN [{_databaseName}].dbo.AR ar ON ar.BillToPatID = p.ID
        JOIN [{_databaseName}].dbo.ARInvoice ai ON ai.ARID = ar.ID
        WHERE
            w.ID IN (SELECT CAST(value AS BIGINT) FROM STRING_SPLIT(@WardIds, ','))
            AND ai.InvoiceDate >= @FromDate
            AND ai.InvoiceDate < DATEADD(DAY, 1, @ToDate)
        GROUP BY p.ID, p.LastName, p.FirstName, w.ID, w.Name;";

    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync(ct);

    var charges = (await connection.QueryAsync<PatientFinancialResponseDto>(
      chargesQuery,
      new { WardIds = wardIdsCsv, FromDate = fromDate, ToDate = toDate })).ToList();

    var clients = (await connection.QueryAsync<ClientSummaryDto>(
      clientSummaryQuery,
      new { WardIds = wardIdsCsv, FromDate = fromDate, ToDate = toDate })).ToList();

    return (charges, clients);
  }

  private async Task<(List<PatientFinancialResponseDto>, List<ClientSummaryDto>)> FetchChargesByPatientIdAsync(
    long patientId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken ct)
  {
    var chargesQuery = $@"
        SELECT
            p.ID AS PatientId,
            ISNULL(w.ID, 0) AS WardId,
            ad.[Date],
            p.LastName + ', ' + p.FirstName AS PatientName,
            p.Address1 AS YourCode,
            ar.AccountNum AS SeamCode,
            ad.Comment AS ChargeDescription,
            CASE WHEN ad.TaxType = 0 THEN 'Exempt' WHEN ad.TaxType = 4 THEN 'HST' ELSE 'Other' END AS TaxType,
            ad.Amount
        FROM [{_databaseName}].dbo.Pat p
        LEFT JOIN [{_databaseName}].dbo.NHWard w ON w.ID = p.NHWardID
        JOIN [{_databaseName}].dbo.AR ar ON ar.BillToPatID = p.ID
        JOIN [{_databaseName}].dbo.ARDetail ad ON ad.ARID = ar.ID
        WHERE
            p.ID = @PatientId
            AND ad.[Date] >= @FromDate
            AND ad.[Date] < DATEADD(DAY, 1, @ToDate)
            AND EXISTS (SELECT 1 FROM [{_databaseName}].dbo.ARPayment ap WHERE ap.ARID = ar.ID AND ap.Status IN (1, 2));";

    var clientSummaryQuery = $@"
        SELECT
            p.ID AS PatientId,
            p.LastName + ', ' + p.FirstName AS PatientName,
            ISNULL(w.ID, 0) AS WardId,
            ISNULL(w.Name, 'N/A') AS LocationHome,
            MAX(ar.AccountNum) AS SeamLessCode,
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
        GROUP BY p.ID, p.LastName, p.FirstName, w.ID, w.Name;";

    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync(ct);

    var charges = (await connection.QueryAsync<PatientFinancialResponseDto>(
      chargesQuery,
      new { PatientId = patientId, FromDate = fromDate, ToDate = toDate })).ToList();

    var clients = (await connection.QueryAsync<ClientSummaryDto>(
      clientSummaryQuery,
      new { PatientId = patientId, FromDate = fromDate, ToDate = toDate })).ToList();

    return (charges, clients);
  }

  private static StatementSheetDto BuildStatementSheetDto(
    string toName,
    string? toAddress,
    List<ClientSummaryDto> clients,
    DateTime fromDate,
    DateTime toDate)
  {
    var period = $"{fromDate:MMMM yyyy}";
    return new StatementSheetDto
    {
      To = new StatementToDto { Name = toName, Address = toAddress },
      From = new StatementFromDto
      {
        BusinessName = null,
        AddressLine1 = null,
        AddressLine2 = null,
        CityProvincePostal = null,
        Tel = null,
        Fax = null,
        HSTNumber = null
      },
      StatementPeriod = period,
      Summary = new StatementSummaryDto
      {
        Details = period,
        SubTotal = clients.Sum(c => c.ChargesOnAccount),
        Tax = clients.Sum(c => c.TaxIncluded),
        PaymentsMade = clients.Sum(c => c.PaymentsMade),
        UnusedCredit = 0,
        PleasePay = clients.Sum(c => c.OutstandingCharges)
      }
    };
  }

  private string SaveExcelFile(
    List<PatientFinancialResponseDto> charges,
    List<ClientSummaryDto> clients,
    string prefix,
    DateTime fromDate,
    DateTime toDate,
    StatementSheetDto statementData)
  {
    var fromOptions = _statementFromOptions;
    statementData.From = new StatementFromDto
    {
      BusinessName = fromOptions.BusinessName,
      AddressLine1 = fromOptions.AddressLine1,
      AddressLine2 = fromOptions.AddressLine2,
      CityProvincePostal = fromOptions.CityProvincePostal,
      Tel = fromOptions.Tel,
      Fax = fromOptions.Fax,
      HSTNumber = fromOptions.HSTNumber
    };

    var folder = Path.Combine("wwwroot", "Invoices");
    Directory.CreateDirectory(folder);
    var fileName = $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
    var fullPath = Path.Combine(folder, fileName);
    var webRoot = Path.GetDirectoryName(folder) ?? folder;

    var bytes = ExcelExportHelper.GenerateOrganizationChargesExcel(
      charges, clients, fromDate, toDate, webRoot, statementData);

    File.WriteAllBytes(fullPath, bytes);
    return $"Invoices/{fileName}";
  }
}
