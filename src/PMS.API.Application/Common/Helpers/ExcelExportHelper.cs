using ClosedXML.Excel;
using PMS.API.Application.Features.Invoice.DTO;
using PMS.API.Application.Features.Patients.DTO;

public static class ExcelExportHelper
{
  private static readonly XLColor PurpleHeader = XLColor.FromHtml("#490757");
  private static readonly XLColor PurpleTableHead = XLColor.FromHtml("#490757");
  //private static readonly XLColor PurpleTableHead = XLColor.FromHtml("#800000");

  // =========================================================
  // ================= CLIENT EXCEL (Individual) =============
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
    string[] headers = { "Patient Name", "Location/Home", "Seamless-Code", "Charges On Account", "Tax Included", "Payments Made", "Outstanding Charges" };

    for (int i = 0; i < headers.Length; i++)
      ws.Cell(1, i + 1).Value = headers[i];

    var range = ws.Range(1, 1, 1, headers.Length);
    range.Style.Font.Bold = true;
    range.Style.Font.FontColor = XLColor.White;
    range.Style.Fill.BackgroundColor = PurpleHeader;
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
    var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
    if (lastRow > 1)
    {
      ws.Range(2, 1, lastRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
      ws.Range(2, 4, lastRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    }
  }

  // =========================================================
  // ================= CHARGES EXCEL (Individual) ============
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
    string[] headers = { "Date", "Patient Name", "Your-Code", "Seam-Code", "Charge Description", "TaxType", "Amount" };
    for (int i = 0; i < headers.Length; i++)
      ws.Cell(1, i + 1).Value = headers[i];

    var range = ws.Range(1, 1, 1, headers.Length);
    range.Style.Font.Bold = true;
    range.Style.Font.FontColor = XLColor.White;
    range.Style.Fill.BackgroundColor = PurpleHeader;
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
    var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
    if (lastRow > 1)
    {
      ws.Range(2, 1, lastRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
      ws.Range(2, 7, lastRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    }
  }

  // =========================================================
  // =========== ORGANIZATION CHARGES EXCEL (Multi-Sheet) ====
  // =========================================================
  public static byte[] GenerateOrganizationChargesExcel(
      List<PatientFinancialResponseDto> chargesData,
      List<ClientSummaryDto> clientsData,
      DateTime fromDate,
      DateTime toDate,
      string webRootPath,
      StatementSheetDto statementData)
  {
    using var workbook = new XLWorkbook();

    // 1. Statement Sheet
    var statementWs = workbook.Worksheets.Add("Statement");
    CreateStatementSheet(statementWs, statementData, fromDate, toDate, webRootPath);

    // 2. Clients Sheet
    var clientsWs = workbook.Worksheets.Add("Clients");
    CreateClientsReportHeader(clientsWs, fromDate, toDate, webRootPath);
    CreateClientsHeader(clientsWs);
    FillClientsData(clientsWs, clientsData);
    FormatClientsWorksheet(clientsWs);

    // 3. Charges Sheet
    var chargesWs = workbook.Worksheets.Add("Charges");
    CreateOrganizationChargesReportHeader(chargesWs, fromDate, toDate, webRootPath);
    CreateOrganizationChargesHeader(chargesWs);
    FillOrganizationChargesData(chargesWs, chargesData);
    FormatOrganizationChargesWorksheet(chargesWs);

    using var ms = new MemoryStream();
    workbook.SaveAs(ms);
    return ms.ToArray();
  }
  private static void CreateStatementSheet(
      IXLWorksheet ws,
      StatementSheetDto statementData,
      DateTime fromDate,
      DateTime toDate,
      string webRootPath)
  {
    ws.Style.Font.FontName = "Calibri";
    ws.Style.Font.FontSize = 11;

    // =====================================================
    // COLUMN WIDTHS
    // =====================================================
    ws.Column(1).Width = 35; // A
    ws.Column(2).Width = 15; // B: Standard Width
    ws.Column(3).Width = 20; // C
    ws.Column(4).Width = 22; // D: From Box Col 1
    ws.Column(5).Width = 22; // E: From Box Col 2
    ws.Column(6).Width = 18; // F

    // =====================================================
    // LOGO
    // =====================================================
    var logoPath = Path.Combine(webRootPath, "Images", "seamlesscare_logo.png");
    if (File.Exists(logoPath))
    {
      var pic = ws.AddPicture(logoPath).MoveTo(ws.Cell(1, 1));
      pic.Scale(0.50);
    }

    // =====================================================
    // PHARMACY SERVICES LINE (Row 4)
    // =====================================================
    // Label in A4 (Standard styling)
    ws.Cell("A4").Value = "Pharmacy Services Statement as of";
    ws.Cell("A4").Style.Fill.BackgroundColor = XLColor.NoColor;
    ws.Cell("A4").Style.Font.FontColor = XLColor.Black;

    // Cell C4: ONLY this cell is Purple with White Text
    var cellC4 = ws.Cell("C4");
    cellC4.Value = $"{fromDate:MMMM yyyy}";
    cellC4.Style.Font.Bold = true;
    cellC4.Style.Font.FontColor = XLColor.White;
    cellC4.Style.Fill.BackgroundColor = PurpleHeader;
    cellC4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

    // =====================================================
    // INTRO TEXT (Selective Bold)
    // =====================================================
    ws.Cell("A5").Value = "Thank you for choosing Seamless Care Pharmacy.";

    var clientRich = ws.Cell("A7").GetRichText();
    clientRich.AddText("Clients: ").SetBold(true);
    clientRich.AddText("Summary of the charges by Patient & Program / Home").SetBold(false);

    var chargeRich = ws.Cell("A8").GetRichText();
    chargeRich.AddText("Charges: ").SetBold(true);
    chargeRich.AddText("Details for all the charges included in this statement").SetBold(false);

    // =====================================================
    // ADDRESS BLOCKS
    // =====================================================
    const int startRow = 12;

    // To Box: Columns A-B
    SetupAddressBlock(ws, startRow, 1, "To:", statementData.To.Name, statementData.To.Address ?? "");

    // From Box: Columns D-E (Modified for 2 columns)
    SetupFromBoxTwoColumns(ws, startRow, 4, "From:", statementData.From);

    // =====================================================
    // STATEMENT SUMMARY (Gap at Row 24)
    // =====================================================
    ws.Cell(23, 1).Value = "Statement Summary";
    ws.Cell(23, 1).Style.Font.Bold = true;

    int tableHeaderRow = 25;

    string[] headers = { "Details", "SubTotal", "Tax", "Payments Made", "Unused Credit", "Please Pay" };
    for (int i = 0; i < headers.Length; i++)
    {
      var cell = ws.Cell(tableHeaderRow, i + 1);
      cell.Value = headers[i];
      cell.Style.Font.Bold = true;
      cell.Style.Font.FontColor = XLColor.White;
      cell.Style.Fill.BackgroundColor = (i == 5) ? PurpleHeader : PurpleTableHead;
      cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
      cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    // Data Row
    var s = statementData.Summary;
    int dataRow = tableHeaderRow + 1;
    ws.Cell(dataRow, 1).Value = s.Details;
    ws.Cell(dataRow, 2).Value = s.SubTotal;
    ws.Cell(dataRow, 3).Value = s.Tax;
    ws.Cell(dataRow, 4).Value = s.PaymentsMade;
    ws.Cell(dataRow, 5).Value = s.UnusedCredit;

    var payCell = ws.Cell(dataRow, 6);
    payCell.Value = s.PleasePay;
    payCell.Style.Fill.BackgroundColor = PurpleHeader;
    payCell.Style.Font.FontColor = XLColor.White;
    payCell.Style.Font.Bold = true;

    ws.Range(dataRow, 2, dataRow, 6).Style.NumberFormat.Format = "$#,##0.00";
    ws.Range(tableHeaderRow, 1, dataRow, 6).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    ws.Range(tableHeaderRow, 1, dataRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

    // =====================================================
    // FOOTER (C31 and C32)
    // =====================================================
    ws.Cell("C31").Value = "For details on the charges please see the Excel Tabs Clients and Charges";
    ws.Cell("C32").Value = "Have any questions? Please email us at accounting@seamlesscare.ca";
  }


  private static void CreateOrganizationChargesReportHeader(IXLWorksheet ws, DateTime fromDate, DateTime toDate, string path)
  {
    var logoPath = Path.Combine(path, "Images", "seamlesscare_logo.png");
    if (File.Exists(logoPath)) ws.AddPicture(logoPath).MoveTo(ws.Cell(1, 1)).Scale(0.5);

    var statementRange = ws.Range(1, 4, 1, 6);
    statementRange.Merge().Value = $"{fromDate:MMM-yy} Statement";
    statementRange.Style.Font.Bold = true;
    statementRange.Style.Font.FontColor = XLColor.White;
    statementRange.Style.Fill.BackgroundColor = PurpleHeader;
    statementRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

    ws.Cell(4, 1).Value = "New Charges for each clients are displayed below.";
    ws.Cell(6, 1).Value = "List of Charges by Each Patient, Program, and Home";
    ws.Cell(6, 1).Style.Font.Bold = true;
  }

  private static void CreateClientsReportHeader(IXLWorksheet ws, DateTime fromDate, DateTime toDate, string path)
  {
    var logoPath = Path.Combine(path, "Images", "seamlesscare_logo.png");
    if (File.Exists(logoPath)) ws.AddPicture(logoPath).MoveTo(ws.Cell(1, 1)).Scale(0.5);

    var range = ws.Range(1, 4, 1, 6);
    range.Merge().Value = $"{fromDate:MMM-yy} Statement";
    range.Style.Font.Bold = true;
    range.Style.Font.FontColor = XLColor.White;
    range.Style.Fill.BackgroundColor = PurpleHeader;
    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

    ws.Cell(4, 1).Value = "New Charges for each clients are displayed below.";
    ws.Cell(6, 1).Value = "Last Month Charges For Each Patient";
    ws.Cell(6, 1).Style.Font.Bold = true;
  }

  private static void CreateOrganizationChargesHeader(IXLWorksheet ws)
  {
    const int headerRow = 8;
    string[] headers = { "Date", "Patient Name", "Your-Code", "Seam-Code", "Charge Description", "TaxType", "Amount" };
    for (int i = 0; i < headers.Length; i++)
    {
      var cell = ws.Cell(headerRow, i + 1);
      cell.Value = headers[i];
      cell.Style.Font.Bold = true;
      cell.Style.Font.FontColor = XLColor.White;
      cell.Style.Fill.BackgroundColor = PurpleTableHead;
      cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }
  }

  private static void FillOrganizationChargesData(IXLWorksheet ws, List<PatientFinancialResponseDto> data)
  {
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

  private static void CreateClientsHeader(IXLWorksheet ws)
  {
    const int row = 8;
    string[] headers = { "Patient Name", "Location/Home", "Seamless-Code", "Charges On Account", "Tax Included", "Payments Made", "Outstanding Charges" };
    for (int i = 0; i < headers.Length; i++)
    {
      var cell = ws.Cell(row, i + 1);
      cell.Value = headers[i];
      cell.Style.Font.Bold = true;
      cell.Style.Font.FontColor = XLColor.White;
      cell.Style.Fill.BackgroundColor = PurpleTableHead;
      cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }
  }

  private static void FillClientsData(IXLWorksheet ws, List<ClientSummaryDto> data)
  {
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
    ws.SheetView.FreezeRows(8);
    var lastRow = ws.LastRowUsed()?.RowNumber() ?? 8;
    if (lastRow >= 9)
    {
      ws.Range(9, 1, lastRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
      ws.Range(9, 4, lastRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
      ws.Range(8, 1, lastRow, 7).SetAutoFilter();
    }
  }

  private static void FormatOrganizationChargesWorksheet(IXLWorksheet ws)
  {
    ws.Columns().AdjustToContents();
    ws.SheetView.FreezeRows(8);
    var lastRow = ws.LastRowUsed()?.RowNumber() ?? 8;
    if (lastRow >= 9)
    {
      ws.Range(9, 1, lastRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
      ws.Range(9, 7, lastRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
      ws.Range(8, 1, lastRow, 7).SetAutoFilter();
    }
  }

  private static void SetupAddressBlock(IXLWorksheet ws, int row, int col, string label, string name, string address)
  {
    // Label Row with PurpleHeader Fill
    var labelRange = ws.Range(row, col, row, col + 1);
    labelRange.Merge().Value = label;
    labelRange.Style.Font.Bold = true;
    labelRange.Style.Font.FontColor = XLColor.White;
    labelRange.Style.Fill.BackgroundColor = PurpleHeader;

    // Patient Name - BOLD
    ws.Cell(row + 1, col).Value = name;
    ws.Cell(row + 1, col).Style.Font.Bold = true;

    ws.Cell(row + 2, col).Value = address;
    ws.Cell(row + 2, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
    ws.Cell(row + 2, col).Style.Alignment.WrapText = true;

    var boxRange = ws.Range(row, col, row + 6, col + 1);
    boxRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
    boxRange.Style.Border.OutsideBorderColor = PurpleHeader;
  }

  private static void SetupAddressFromBlock(IXLWorksheet ws, int row, int col, string label, StatementFromDto from)
  {
    // Inner Header Fill
    var headerRange = ws.Range(row, col, row, col + 2);
    headerRange.Merge().Value = label;
    headerRange.Style.Font.Bold = true;
    headerRange.Style.Font.FontColor = XLColor.White;
    headerRange.Style.Fill.BackgroundColor = PurpleHeader;

    // Content
    ws.Cell(row + 1, col).Value = from.BusinessName;
    ws.Cell(row + 2, col).Value = from.AddressLine1;
    ws.Cell(row + 3, col).Value = from.AddressLine2;
    ws.Cell(row + 4, col).Value = from.CityProvincePostal;
    ws.Cell(row + 5, col).Value = $"Tel: {from.Tel}";
    ws.Cell(row + 6, col).Value = $"Fax: {from.Fax}";
    ws.Cell(row + 7, col).Value = $"HST# {from.HSTNumber}";

    // Outer Border for D-F Box
    var boxRange = ws.Range(row, col, row + 7, col + 2);
    boxRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
    boxRange.Style.Border.OutsideBorderColor = PurpleHeader;
    boxRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
  }
  private static void SetupFromBoxTwoColumns(IXLWorksheet ws, int row, int col, string label, StatementFromDto from)
  {
    // Header Bar (D-E)
    var headerRange = ws.Range(row, col, row, col + 1);
    headerRange.Merge().Value = label;
    headerRange.Style.Font.Bold = true;
    headerRange.Style.Font.FontColor = XLColor.White;
    headerRange.Style.Fill.BackgroundColor = PurpleHeader;

    ws.Cell(row + 1, col).Value = from.BusinessName;
    ws.Cell(row + 2, col).Value = from.AddressLine1;
    ws.Cell(row + 3, col).Value = from.AddressLine2;
    ws.Cell(row + 4, col).Value = from.CityProvincePostal;
    ws.Cell(row + 5, col).Value = $"Tel: {from.Tel}";
    ws.Cell(row + 6, col).Value = $"Fax: {from.Fax}";
    ws.Cell(row + 7, col).Value = $"HST# {from.HSTNumber}";

    // Border around D-E
    var boxRange = ws.Range(row, col, row + 8, col + 1);
    boxRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
    boxRange.Style.Border.OutsideBorderColor = PurpleHeader;
  }
}
