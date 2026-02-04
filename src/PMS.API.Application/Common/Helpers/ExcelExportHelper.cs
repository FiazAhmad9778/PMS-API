using ClosedXML.Excel;
using PMS.API.Application.Features.Invoice.DTO;
using PMS.API.Application.Features.Patients.DTO;
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
    range.Style.Fill.BackgroundColor = XLColor.FromArgb(73, 7, 87);
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
    range.Style.Fill.BackgroundColor = XLColor.FromArgb(73, 7, 87);
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

  // =========================================================
  // =========== ORGANIZATION CHARGES EXCEL ==================
  // =========================================================
  public static byte[] GenerateOrganizationChargesExcel(List<PatientFinancialResponseDto> chargesData, List<ClientSummaryDto> clientsData, DateTime fromDate, DateTime toDate, string path, StatementSheetDto statementData)
  {
    using var workbook = new XLWorkbook();

    // Create Statement worksheet first (first tab)
    var statementWs = workbook.Worksheets.Add("Statement");
    CreateStatementSheet(statementWs, statementData, fromDate, toDate, path);

    // Create Clients worksheet
    var clientsWs = workbook.Worksheets.Add("Clients");
    CreateClientsReportHeader(clientsWs, fromDate, toDate, path);
    CreateClientsHeader(clientsWs);
    FillClientsData(clientsWs, clientsData);
    FormatClientsWorksheet(clientsWs);

    // Create Charges worksheet
    var chargesWs = workbook.Worksheets.Add("Charges");
    CreateOrganizationChargesReportHeader(chargesWs, fromDate, toDate, path);
    CreateOrganizationChargesHeader(chargesWs);
    FillOrganizationChargesData(chargesWs, chargesData);
    FormatOrganizationChargesWorksheet(chargesWs);

    using var ms = new MemoryStream();
    workbook.SaveAs(ms);
    return ms.ToArray();
  }

  private static void CreateStatementSheet(IXLWorksheet ws, StatementSheetDto statementData, DateTime fromDate, DateTime toDate, string webRootPath)
  {
    var purple = XLColor.FromArgb(73, 7, 87);
    var purpleTable = XLColor.FromArgb(102, 0, 102);

    // Logo (same as Clients/Charges sheets – A1, scale 0.5)
    var logoPath = Path.Combine(webRootPath, "Images", "seamlesscare_logo.png");
    if (File.Exists(logoPath))
      ws.AddPicture(logoPath).MoveTo(ws.Cell(1, 1)).Scale(0.5);
    else
    {
      ws.Cell(1, 1).Value = "Seamless Care";
      ws.Cell(1, 1).Style.Font.FontSize = 16;
      ws.Cell(1, 1).Style.Font.Bold = true;
      ws.Cell(1, 1).Style.Font.FontColor = purple;
      ws.Cell(2, 1).Value = "Pharmacy";
      ws.Cell(2, 1).Style.Font.FontSize = 12;
      ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromArgb(180, 120, 100);
    }

    // Date/title (same as other sheets – D1:F1 merged, "MMM-yy Statement", purple, white, centered)
    var statementRange = ws.Range(1, 4, 1, 6);
    statementRange.Merge();
    statementRange.Value = $"{fromDate:MMM-yy} Statement";
    statementRange.Style.Font.Bold = true;
    statementRange.Style.Font.FontColor = XLColor.White;
    statementRange.Style.Fill.BackgroundColor = purple;
    statementRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    statementRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

    // Intro text
    ws.Cell(4, 1).Value = "Thank you for your business. Please find below a summary of your statement.";
    ws.Cell(6, 1).Value = "Clients: Summary of the charges by Patient & Program / Home";
    ws.Cell(8, 1).Value = "Charges: Details for all the charges included in this statement.";

    // TO block (left) - rows 12-20
    const int toRowStart = 12;
    ws.Cell(toRowStart, 1).Value = "To:";
    ws.Cell(toRowStart, 1).Style.Font.Bold = true;
    ws.Cell(toRowStart, 1).Style.Font.FontColor = purple;
    ws.Cell(toRowStart + 1, 1).Value = statementData.To.Name;
    if (!string.IsNullOrWhiteSpace(statementData.To.Address))
    {
      var addressLines = statementData.To.Address.Replace("\r\n", "\n").Split('\n');
      for (int i = 0; i < addressLines.Length; i++)
        ws.Cell(toRowStart + 2 + i, 1).Value = addressLines[i].Trim();
    }
    var toBoxEndRow = toRowStart + 6;
    ws.Range(toRowStart, 1, toBoxEndRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
    ws.Range(toRowStart, 1, toBoxEndRow, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    ws.Range(toRowStart, 1, toBoxEndRow, 3).Style.Border.OutsideBorderColor = purple;

    // FROM block (right) - same rows
    ws.Cell(toRowStart, 4).Value = "From:";
    ws.Cell(toRowStart, 4).Style.Font.Bold = true;
    ws.Cell(toRowStart, 4).Style.Font.FontColor = purple;
    var from = statementData.From;
    int fromRow = toRowStart + 1;
    if (!string.IsNullOrWhiteSpace(from.BusinessName))
      ws.Cell(fromRow++, 4).Value = from.BusinessName;
    if (!string.IsNullOrWhiteSpace(from.AddressLine1))
      ws.Cell(fromRow++, 4).Value = from.AddressLine1;
    if (!string.IsNullOrWhiteSpace(from.AddressLine2))
      ws.Cell(fromRow++, 4).Value = from.AddressLine2;
    if (!string.IsNullOrWhiteSpace(from.CityProvincePostal))
      ws.Cell(fromRow++, 4).Value = from.CityProvincePostal;
    if (!string.IsNullOrWhiteSpace(from.Tel))
      ws.Cell(fromRow++, 4).Value = "Tel: " + from.Tel;
    if (!string.IsNullOrWhiteSpace(from.Fax))
      ws.Cell(fromRow++, 4).Value = "Fax: " + from.Fax;
    if (!string.IsNullOrWhiteSpace(from.HSTNumber))
      ws.Cell(fromRow++, 4).Value = "HST# " + from.HSTNumber;
    ws.Range(toRowStart, 4, toBoxEndRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
    ws.Range(toRowStart, 4, toBoxEndRow, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    ws.Range(toRowStart, 4, toBoxEndRow, 7).Style.Border.OutsideBorderColor = purple;

    // Statement Summary table (same header style as Clients/Charges – row 8 purple, white, centered)
    const int summaryRowStart = 23;
    ws.Cell(summaryRowStart, 1).Value = "Statement Summary";
    ws.Cell(summaryRowStart, 1).Style.Font.Bold = true;
    const int summaryHeaderRow = summaryRowStart + 1;
    string[] summaryHeaders = { "Details", "SubTotal", "Tax", "Payments Made", "Unused Credit", "Please Pay" };
    for (int i = 0; i < summaryHeaders.Length; i++)
      ws.Cell(summaryHeaderRow, i + 1).Value = summaryHeaders[i];
    var summaryHeaderRange = ws.Range(summaryHeaderRow, 1, summaryHeaderRow, summaryHeaders.Length);
    summaryHeaderRange.Style.Font.Bold = true;
    summaryHeaderRange.Style.Font.FontColor = XLColor.White;
    summaryHeaderRange.Style.Fill.BackgroundColor = purpleTable;
    summaryHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    summaryHeaderRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

    var s = statementData.Summary;
    const int summaryDataRow = summaryHeaderRow + 1;
    ws.Cell(summaryDataRow, 1).Value = s.Details;
    ws.Cell(summaryDataRow, 2).Value = s.SubTotal;
    ws.Cell(summaryDataRow, 3).Value = s.Tax;
    ws.Cell(summaryDataRow, 4).Value = s.PaymentsMade;
    ws.Cell(summaryDataRow, 5).Value = s.UnusedCredit;
    ws.Cell(summaryDataRow, 6).Value = s.PleasePay;
    ws.Range(summaryDataRow, 2, summaryDataRow, 6).Style.NumberFormat.Format = "$#,##0.00";
    ws.Range(summaryDataRow, 2, summaryDataRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    ws.Range(summaryHeaderRow, 1, summaryDataRow, summaryHeaders.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

    // Footer note
    ws.Cell(summaryDataRow + 3, 2).Value = "Please check the Clients and Charges tabs for details. Questions? accounting@seamlesscare.ca";

    ws.Columns().AdjustToContents();
  }

  // Create report header section (logo, statement title, intro text) for Charges worksheet
  private static void CreateOrganizationChargesReportHeader(IXLWorksheet ws, DateTime fromDate, DateTime toDate, string path)
  {
    // Insert Seamless Care logo image
    // Make sure the logo image is in your project, e.g., "wwwroot/images/seamlesscare_logo.png"
    // and its Build Action is "Content" and "Copy to Output Directory" is set to "Copy if newer"
    var logoPath = Path.Combine(path, "Images", "seamlesscare_logo.png");
    if (File.Exists(logoPath))
    {
      var picture = ws.AddPicture(logoPath)
                      .MoveTo(ws.Cell(1, 1))  // Position at cell A1
                      .Scale(0.5);            // Adjust the scale if needed
    }

    // Statement title (D1-F1 merged, purple background)
    var statementRange = ws.Range(1, 4, 1, 6);
    statementRange.Merge();
    var statementText = $"{fromDate:MMM-yy} Statement";
    statementRange.Value = statementText;
    statementRange.Style.Font.Bold = true;
    statementRange.Style.Font.FontColor = XLColor.White;
    statementRange.Style.Fill.BackgroundColor = XLColor.FromArgb(73, 7, 87);
    statementRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    statementRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

    // Introductory text
    ws.Cell(4, 1).Value = "New Charges for each clients are displayed below.";
    ws.Cell(6, 1).Value = "List of Charges by Each Patient, Program, and Home";
    ws.Cell(6, 1).Style.Font.Bold = true;
  }


  // Create report header section for Clients worksheet
  private static void CreateClientsReportHeader(IXLWorksheet ws, DateTime fromDate, DateTime toDate, string webRootPath)
  {
    // Logo for Clients (instead of "Seamless Care / Pharmacy" text)
    var logoPath = Path.Combine(webRootPath, "Images", "seamlesscare_logo.png");
    if (File.Exists(logoPath))
    {
      ws.AddPicture(logoPath)
        .MoveTo(ws.Cell(1, 1))  // Place logo at A1
        .Scale(0.5);             // Adjust scale if needed
    }
    else
    {
      // Fallback text if logo not found
      ws.Cell(1, 1).Value = "Seamless Care";
      ws.Cell(1, 1).Style.Font.FontSize = 16;
      ws.Cell(1, 1).Style.Font.Bold = true;
      ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromArgb(73, 7, 87);

      ws.Cell(2, 1).Value = "Pharmacy";
      ws.Cell(2, 1).Style.Font.FontSize = 12;
      ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromArgb(180, 120, 100);
    }

    // Statement title (D1-F1 merged, purple background)
    var statementRange = ws.Range(1, 4, 1, 6);
    statementRange.Merge();
    var statementText = $"{fromDate:MMM-yy} Statement";
    statementRange.Value = statementText;
    statementRange.Style.Font.Bold = true;
    statementRange.Style.Font.FontColor = XLColor.White;
    statementRange.Style.Fill.BackgroundColor = XLColor.FromArgb(73, 7, 87); // HEX #490757
    statementRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    statementRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

    // Introductory text
    ws.Cell(4, 1).Value = "New Charges for each clients are displayed below.";
    ws.Cell(6, 1).Value = "Last Month Charges For Each Patient";
    ws.Cell(6, 1).Style.Font.Bold = true;
  }


  private static void CreateOrganizationChargesHeader(IXLWorksheet ws)
  {
    // Data table starts at row 8 (after header sections)
    const int headerRow = 8;
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
      ws.Cell(headerRow, i + 1).Value = headers[i];

    var range = ws.Range(headerRow, 1, headerRow, headers.Length);
    range.Style.Font.Bold = true;
    range.Style.Font.FontColor = XLColor.White;
    range.Style.Fill.BackgroundColor = XLColor.FromArgb(102, 0, 102);
    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
  }

  private static void FillOrganizationChargesData(IXLWorksheet ws, List<PatientFinancialResponseDto> data)
  {
    // Data starts at row 9 (after header row at row 8)
    int row = 9;

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

  private static void FormatOrganizationChargesWorksheet(IXLWorksheet ws)
  {
    ws.Columns().AdjustToContents();
    ws.SheetView.FreezeRows(8); // Freeze at header row (row 8)

    var lastRow = ws.LastRowUsed()!.RowNumber();
    const int headerRow = 8;
    const int dataStartRow = 9;
    
    if (lastRow >= dataStartRow)
    {
      // Apply borders to data rows
      ws.Range(dataStartRow, 1, lastRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
      ws.Range(dataStartRow, 7, lastRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
      
      // Enable auto-filters on all columns (header row + data rows)
      ws.Range(headerRow, 1, lastRow, 7).SetAutoFilter();
    }
    else if (lastRow == headerRow)
    {
      // Even if only header row exists, enable filters
      ws.Range(headerRow, 1, headerRow, 7).SetAutoFilter();
    }
  }

  // =========================================================
  // =========== CLIENTS WORKSHEET ==========================
  // =========================================================
  private static void CreateClientsHeader(IXLWorksheet ws)
  {
    // Data table starts at row 8 (after header sections)
    const int headerRow = 8;
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
      ws.Cell(headerRow, i + 1).Value = headers[i];

    var range = ws.Range(headerRow, 1, headerRow, headers.Length);
    range.Style.Font.Bold = true;
    range.Style.Font.FontColor = XLColor.White;
    range.Style.Fill.BackgroundColor = XLColor.FromArgb(102, 0, 102);
    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
  }

  private static void FillClientsData(IXLWorksheet ws, List<ClientSummaryDto> data)
  {
    // Data starts at row 9 (after header row at row 8)
    int row = 9;

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

  private static void FormatClientsWorksheet(IXLWorksheet ws)
  {
    ws.Columns().AdjustToContents();
    ws.SheetView.FreezeRows(8); // Freeze at header row (row 8)

    var lastRow = ws.LastRowUsed()!.RowNumber();
    const int headerRow = 8;
    const int dataStartRow = 9;
    
    if (lastRow >= dataStartRow)
    {
      // Apply borders to data rows
      ws.Range(dataStartRow, 1, lastRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
      ws.Range(dataStartRow, 4, lastRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
      
      // Enable auto-filters on all columns (header row + data rows)
      ws.Range(headerRow, 1, lastRow, 7).SetAutoFilter();
    }
    else if (lastRow == headerRow)
    {
      // Even if only header row exists, enable filters
      ws.Range(headerRow, 1, headerRow, 7).SetAutoFilter();
    }
  }
}
