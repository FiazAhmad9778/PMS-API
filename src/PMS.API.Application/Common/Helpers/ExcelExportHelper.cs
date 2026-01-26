using ClosedXML.Excel;
using PMS.API.Application.Features.Invoice.DTO;
using System.Drawing;

public static class ExcelExportHelper
{
  // =========================================================
  // ================= CLIENT EXCEL ==========================
  // =========================================================
  public static byte[] GenerateClientExcel(List<ClientReportDto> data)
  {
    using var workbook = new XLWorkbook();
    var ws = workbook.Worksheets.Add("Client Summary");

    CreateClientHeader(ws);
    FillClientData(ws, data);
    FormatClientWorksheet(ws);

    using var ms = new MemoryStream();
    workbook.SaveAs(ms);
    return ms.ToArray();
  }

  private static void CreateClientHeader(IXLWorksheet ws)
  {
    string[] headers =
    {
            "Patient Name",
            "Location/Home",
            "Seamless-Code",
            "Charges On Account",
            "Tax Included",
            "Payments Made",
            "Outstanding Charges"
        };

    for (int i = 0; i < headers.Length; i++)
      ws.Cell(1, i + 1).Value = headers[i];

    var range = ws.Range(1, 1, 1, headers.Length);
    range.Style.Font.Bold = true;
    range.Style.Font.FontColor = XLColor.White;
    range.Style.Fill.BackgroundColor = XLColor.FromArgb(102, 0, 102);
    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
  }

  private static void FillClientData(IXLWorksheet ws, List<ClientReportDto> data)
  {
    int row = 2;

    foreach (var item in data)
    {
      ws.Cell(row, 1).Value = item.PatientName;
      ws.Cell(row, 2).Value = item.LocationHome;
      ws.Cell(row, 3).Value = item.SeamLessCode;

      ws.Cell(row, 4).Value = item.ChargesOnAccount;
      ws.Cell(row, 5).Value = item.TaxIncluded;
      ws.Cell(row, 6).Value = item.PaymentsMade;
      ws.Cell(row, 7).Value = item.OutstandingCharges;

      ws.Range(row, 4, row, 7).Style.NumberFormat.Format = "$#,##0.00";

      row++;
    }
  }

  private static void FormatClientWorksheet(IXLWorksheet ws)
  {
    ws.Columns().AdjustToContents();
    ws.SheetView.FreezeRows(1);

    var lastRow = ws.LastRowUsed()!.RowNumber();
    ws.Range(2, 1, lastRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    ws.Range(2, 4, lastRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
  }

  // =========================================================
  // ================= CHARGES EXCEL =========================
  // =========================================================
  public static byte[] GenerateChargesExcel(List<ChargesReportDto> data)
  {
    using var workbook = new XLWorkbook();
    var ws = workbook.Worksheets.Add("Charges Detail");

    CreateChargesHeader(ws);
    FillChargesData(ws, data);
    FormatChargesWorksheet(ws);

    using var ms = new MemoryStream();
    workbook.SaveAs(ms);
    return ms.ToArray();
  }

  private static void CreateChargesHeader(IXLWorksheet ws)
  {
    string[] headers =
    {
            "Date",
            "Patient Name",
            "Your-Code",
            "Seam-Code",
            "Charge Description",
            "TaxType",
            "Amount"
        };

    for (int i = 0; i < headers.Length; i++)
      ws.Cell(1, i + 1).Value = headers[i];

    var range = ws.Range(1, 1, 1, headers.Length);
    range.Style.Font.Bold = true;
    range.Style.Font.FontColor = XLColor.White;
    range.Style.Fill.BackgroundColor = XLColor.FromArgb(102, 0, 102);
    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
  }

  private static void FillChargesData(IXLWorksheet ws, List<ChargesReportDto> data)
  {
    int row = 2;

    foreach (var item in data)
    {
      ws.Cell(row, 1).Value = item.Date;
      ws.Cell(row, 1).Style.DateFormat.Format = "yyyy-MM-dd";

      ws.Cell(row, 2).Value = item.PatientName;
      ws.Cell(row, 3).Value = item.YourCode;
      ws.Cell(row, 4).Value = item.SeamCode;
      ws.Cell(row, 5).Value = item.ChargeDescription;
      ws.Cell(row, 6).Value = item.TaxType;

      ws.Cell(row, 7).Value = item.Amount;
      ws.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";

      row++;
    }
  }

  private static void FormatChargesWorksheet(IXLWorksheet ws)
  {
    ws.Columns().AdjustToContents();
    ws.SheetView.FreezeRows(1);

    var lastRow = ws.LastRowUsed()!.RowNumber();
    ws.Range(2, 1, lastRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    ws.Range(2, 7, lastRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
  }
}
