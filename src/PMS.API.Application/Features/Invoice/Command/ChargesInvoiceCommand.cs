using System.ComponentModel.DataAnnotations;
using System.Data;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Invoice.DTO;

namespace PMS.API.Application.Features.Invoice.Command;

public class ChargesInvoiceCommand : IRequest<ApplicationResult<List<ChargesReportDto>>>
{
  public long? OrganizationId { get; set; }
  public long? WardId { get; set; }

  [Required]
  public long PatientId { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public DateTime EndDate { get; set; }
}


  #region SP

  //public async Task<ApplicationResult<List<ChargesReportDto>>> Handle(
  //    ChargesInvoiceCommand request,
  //    CancellationToken cancellationToken)
  //{
  //  try
  //  {
  //    using var connection = new SqlConnection(
  //        _configuration.GetConnectionString("PMSConnection"));

  //    var parameters = new DynamicParameters();
  //    parameters.Add("@NHID", request.OrganizationId);
  //    parameters.Add("@WardID", request.WardId);
  //    parameters.Add("@PatientID", request.PatientId);
  //    parameters.Add("@FromDate", request.StartDate.Date);
  //    parameters.Add("@ToDate", request.EndDate.Date);

  //    var data = (await connection.QueryAsync<ChargesReportDto>(
  //        "dbo.SP_Charges_V4",
  //        parameters,
  //        commandType: CommandType.StoredProcedure))
  //        .ToList();

  //    return ApplicationResult<List<ChargesReportDto>>
  //        .SuccessResult(data);
  //  }
  //  catch (Exception ex)
  //  {
  //    return ApplicationResult<List<ChargesReportDto>>.Error(
  //        new[] { "Failed to fetch charges invoice." },
  //        ex
  //    );
  //  }
  //}

  #endregion

  #region RAW QUERY

  public class ChargesInvoiceCommandHandler
    : IRequestHandler<ChargesInvoiceCommand, ApplicationResult<List<ChargesReportDto>>>
  {
    private readonly IConfiguration _configuration;

    public ChargesInvoiceCommandHandler(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public async Task<ApplicationResult<List<ChargesReportDto>>> Handle(
        ChargesInvoiceCommand request,
        CancellationToken cancellationToken)
    {
      try
      {
        const string sql = @"
SELECT
    ad.[Date] AS [Date],
    p.LastName + ', ' + p.FirstName AS PatientName,
    w.Name AS YourCode,
    ar.AccountNum AS SeamCode,
    ad.Comment AS ChargeDescription,
    CASE ad.TaxType
        WHEN 0 THEN 'Exempt'
        WHEN 4 THEN 'HST'
        ELSE 'Other'
    END AS TaxType,
    ad.Amount AS Amount
FROM dbo.NHWard w
JOIN dbo.Pat p ON p.NHWardID = w.ID
JOIN dbo.AR ar ON ar.BillToPatID = p.ID
JOIN dbo.ARDetail ad ON ad.ARID = ar.ID AND ad.PatID = p.ID
WHERE
    EXISTS (
        SELECT 1
        FROM dbo.ARPayment ap
        WHERE ap.ARID = ar.ID
          AND ap.Status IN (1, 2)
    )
    AND (@NHID IS NULL OR w.NHID = @NHID)
    AND (@WardID IS NULL OR w.ID = @WardID)
    AND (@PatientID IS NULL OR p.ID = @PatientID)
    AND ad.[Date] >= @FromDate
    AND ad.[Date] < DATEADD(DAY, 1, @ToDate)
ORDER BY
    ad.[Date],
    p.LastName,
    p.FirstName;";

        using var connection = new SqlConnection(
            _configuration.GetConnectionString("PMSConnection"));

        var data = (await connection.QueryAsync<ChargesReportDto>(
            sql,
            new
            {
              NHID = request.OrganizationId,
              WardID = request.WardId,
              PatientID = request.PatientId,
              FromDate = request.StartDate.Date,
              ToDate = request.EndDate.Date
            }))
            .ToList();

        if (!data.Any())
        {
          return ApplicationResult<List<ChargesReportDto>>
              .Error("No charges found for the selected period.");
        }

        return ApplicationResult<List<ChargesReportDto>>
            .SuccessResult(data);
      }
      catch (Exception ex)
      {
        return ApplicationResult<List<ChargesReportDto>>.Error(
            new[] { "Failed to fetch charges invoice." },
            ex
        );
      }
    }
  }


  #endregion



