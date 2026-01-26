using System.ComponentModel.DataAnnotations;
using Dapper;
using System.Data;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Invoice.DTO;

namespace PMS.API.Application.Features.Invoice.Command;
public class ClientInvoiceCommand : IRequest<ApplicationResult<List<ClientReportDto>>>
{
  [Required]
  public long PatientId { get; set; }
  [Required]
  public DateTime FromDate { get; set; }
  [Required]
  public DateTime ToDate { get; set; }
}


#region SP

//public async Task<ApplicationResult<List<ClientReportDto>>> Handle (ClientInvoiceCommand request, CancellationToken cancellationToken)
//{
//  try
//  {
//    using var connection = new SqlConnection(
//        _configuration.GetConnectionString("PMSConnection"));

//    var parameters = new DynamicParameters();
//    parameters.Add("@PatientID", request.PatientId);
//    parameters.Add("@FromDate", request.FromDate.Date);
//    parameters.Add("@ToDate", request.ToDate.Date);

//    var data = (await connection.QueryAsync<ClientReportDto>(
//        "dbo.SP_Client_V1",
//        parameters,
//        commandType: CommandType.StoredProcedure))
//        .ToList();

//    if (!data.Any())
//    {
//      return ApplicationResult<List<ClientReportDto>>.Error(
//          "No invoice data found for the selected period.");
//    }

//    return ApplicationResult<List<ClientReportDto>>.SuccessResult(data);
//  }
//  catch (Exception ex)
//  {
//    return ApplicationResult<List<ClientReportDto>>.Error(
//        new[] { "Failed to generate client invoice report." },
//        ex
//    );
//  }
//}

#endregion

#region RAW QUERY
public class ClientInvoiceCommandHandler : IRequestHandler<ClientInvoiceCommand, ApplicationResult<List<ClientReportDto>>>
{
  private readonly IConfiguration _configuration;

  public ClientInvoiceCommandHandler(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public async Task<ApplicationResult<List<ClientReportDto>>> Handle(
      ClientInvoiceCommand request,
      CancellationToken cancellationToken)
  {
    try
    {
      const string sql = @"
SELECT
    p.LastName + ', ' + p.FirstName AS PatientName,
    w.Name AS LocationHome,
    ar.AccountNum AS SeamLessCode,
    SUM(ISNULL(ai.SubTotal, 0)) AS ChargesOnAccount,
    SUM(ISNULL(ai.Tax1, 0) + ISNULL(ai.Tax2, 0)) AS TaxIncluded,
    CAST(SUM(ISNULL(ai.PaymentPending, 0)) AS NUMERIC(18,2)) AS PaymentsMade,
    SUM(ISNULL(ai.Paid, 0)) AS OutstandingCharges
FROM dbo.Pat p
INNER JOIN dbo.NHWard w ON w.ID = p.NHWardID
INNER JOIN dbo.AR ar ON ar.BillToPatId = p.ID
INNER JOIN dbo.ARInvoice ai ON ai.ARID = ar.ID
WHERE
    p.ID = @PatientID
    AND ai.InvoiceDate >= @FromDate
    AND ai.InvoiceDate < DATEADD(DAY, 1, @ToDate)
GROUP BY
    p.LastName,
    p.FirstName,
    w.Name,
    ar.AccountNum
ORDER BY
    p.LastName,
    p.FirstName;";

      using var connection = new SqlConnection(
          _configuration.GetConnectionString("PMSConnection"));

      var data = (await connection.QueryAsync<ClientReportDto>(
          sql,
          new
          {
            request.PatientId,
            FromDate = request.FromDate.Date,
            ToDate = request.ToDate.Date
          }))
          .ToList();

      if (!data.Any())
      {
        return ApplicationResult<List<ClientReportDto>>
            .Error("No invoice data found for the selected period.");
      }

      return ApplicationResult<List<ClientReportDto>>
          .SuccessResult(data);
    }
    catch (Exception ex)
    {
      return ApplicationResult<List<ClientReportDto>>.Error(
          new[] { "Failed to generate client invoice report." },
          ex
      );
    }
  }
}

#endregion
