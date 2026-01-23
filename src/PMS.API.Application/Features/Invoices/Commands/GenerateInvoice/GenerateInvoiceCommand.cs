using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.Text;

namespace PMS.API.Application.Features.Invoices.Commands.GenerateInvoice;

public class GenerateInvoiceCommand : IRequest<ApplicationResult<byte[]>>
{
  [Required]
  public required string PatientId { get; set; }

  public string? PatientName { get; set; }
  public string? OrganizationName { get; set; }
  public string? Address { get; set; }
  public string? StatementDate { get; set; }
}

public class GenerateInvoiceCommandHandler : RequestHandlerBase<GenerateInvoiceCommand, ApplicationResult<byte[]>>
{
  public GenerateInvoiceCommandHandler(
    IServiceProvider serviceProvider,
    ILogger<GenerateInvoiceCommandHandler> logger) : base(serviceProvider, logger)
  {
  }

  protected override async Task<ApplicationResult<byte[]>> HandleRequest(GenerateInvoiceCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Run CPU-bound PDF generation on a background thread
      var pdfBytes = await Task.Run(() =>
      {
        // Create a new PDF document
        var document = new PdfDocument();
        var page = document.AddPage();
        page.Size = PageSize.Letter;
        var gfx = XGraphics.FromPdfPage(page);

        // Set up fonts
        var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
        var headingFont = new XFont("Arial", 12, XFontStyle.Bold);
        var normalFont = new XFont("Arial", 10, XFontStyle.Regular);
        var smallFont = new XFont("Arial", 9, XFontStyle.Regular);

        var pageWidth = page.Width;
        var pageHeight = page.Height;
        var margin = 50;
        double currentY = margin;

        // Logo placeholder (top left)
        var logoRect = new XRect(margin, currentY, 100, 40);
        gfx.DrawRectangle(XBrushes.White, logoRect);
        gfx.DrawRectangle(XPens.Black, logoRect);
        gfx.DrawString("LOGO", smallFont, XBrushes.Gray, logoRect, XStringFormats.Center);

        // Header (top right)
        var headerRect = new XRect(pageWidth - margin - 200, currentY, 200, 60);
        gfx.DrawString("Pharmacy Services Statement", titleFont, XBrushes.Black, 
          new XRect(pageWidth - margin - 200, currentY, 200, 30), XStringFormats.TopRight);
        
        var statementDate = !string.IsNullOrEmpty(request.StatementDate) 
          ? request.StatementDate 
          : DateTime.Now.ToString("MMMM yyyy");
        gfx.DrawString($"as of {statementDate}", normalFont, XBrushes.Black,
          new XRect(pageWidth - margin - 200, currentY + 30, 200, 20), XStringFormats.TopRight);

        currentY += 80;

        // Thank you message
        gfx.DrawString("Thank you for choosing Seamless Care Pharmacy.", normalFont, XBrushes.Black,
          new XRect(margin, currentY, pageWidth - 2 * margin, 20), XStringFormats.TopLeft);
        currentY += 25;

        gfx.DrawString(
          "Please see below the statement of your account. See the details below with the summary of the charges by Patient & Program / Home and the details for all charges included in this statement.",
          normalFont, XBrushes.Black,
          new XRect(margin, currentY, pageWidth - 2 * margin, 40), XStringFormats.TopLeft);
        currentY += 50;

        // To and From sections
        var sectionWidth = (pageWidth - 3 * margin) / 2;

        // To section
        double toStartY = currentY;
        gfx.DrawString("To:", headingFont, XBrushes.Black,
          new XRect(margin, currentY, sectionWidth, 20), XStringFormats.TopLeft);
        currentY += 20;

        if (!string.IsNullOrEmpty(request.PatientName))
        {
          gfx.DrawString(request.PatientName, normalFont, XBrushes.Black,
            new XRect(margin, currentY, sectionWidth, 20), XStringFormats.TopLeft);
          currentY += 20;
        }

        var addressLines = new string[0];
        if (!string.IsNullOrEmpty(request.Address))
        {
          addressLines = request.Address.Split('\n');
          foreach (var line in addressLines)
          {
            gfx.DrawString(line, normalFont, XBrushes.Black,
              new XRect(margin, currentY, sectionWidth, 20), XStringFormats.TopLeft);
            currentY += 20;
          }
        }
        else
        {
          gfx.DrawString("Address not available", smallFont, XBrushes.Gray,
            new XRect(margin, currentY, sectionWidth, 20), XStringFormats.TopLeft);
          currentY += 20;
        }

        // From section (right side) - align with To section
        double fromY = toStartY;
        gfx.DrawString("From:", headingFont, XBrushes.Black,
          new XRect(margin + sectionWidth + margin, fromY, sectionWidth, 20), XStringFormats.TopLeft);
        fromY += 20;

        var fromInfo = new[]
        {
          "Seamless Care Pharmacy",
          "15 Grand Marshall Dr - Unit 3",
          "Scarborough ON M1B 5N6",
          "Tel: (877) 666-9502",
          "Fax: (877) 666-0902",
          "HST# 888258779RT0001"
        };

        foreach (var line in fromInfo)
        {
          gfx.DrawString(line, normalFont, XBrushes.Black,
            new XRect(margin + sectionWidth + margin, fromY, sectionWidth, 20), XStringFormats.TopLeft);
          fromY += 20;
        }

        currentY = Math.Max(currentY, fromY) + 30;

        // Statement Summary Table
        gfx.DrawString("Statement Summary", headingFont, XBrushes.Black,
          new XRect(margin, currentY, pageWidth - 2 * margin, 20), XStringFormats.TopLeft);
        currentY += 30;

        // Table headers
        var tableX = margin;
        var tableWidth = pageWidth - 2 * margin;
        var colWidths = new[] { 2.0, 1.0, 1.0, 1.0, 1.0, 1.0 }; // Relative widths
        var totalWidth = colWidths.Sum();
        var actualColWidths = colWidths.Select(w => (tableWidth * w / totalWidth)).ToArray();

        var headers = new[] { "Details", "SubTotal", "Tax", "Payments Made", "Unused Credit", "Please Pay" };
        double headerY = currentY;
        var rowHeight = 30;

        // Draw header background
        gfx.DrawRectangle(XBrushes.LightGray, new XRect(tableX, headerY, tableWidth, rowHeight));
        gfx.DrawRectangle(XPens.Black, new XRect(tableX, headerY, tableWidth, rowHeight));

        // Draw header text
        double headerX = tableX;
        for (int i = 0; i < headers.Length; i++)
        {
          var rect = new XRect(headerX, headerY, actualColWidths[i], rowHeight);
          gfx.DrawRectangle(XPens.Black, rect);
          var format = i == 0 ? XStringFormats.CenterLeft : XStringFormats.CenterRight;
          gfx.DrawString(headers[i], smallFont, XBrushes.Black, rect, format);
          headerX += actualColWidths[i];
        }

        currentY += rowHeight;

        // Table row
        double rowY = currentY;
        gfx.DrawRectangle(XPens.Black, new XRect(tableX, rowY, tableWidth, rowHeight));

        var detailText = request.PatientName ?? "Patient";
        if (!string.IsNullOrEmpty(request.OrganizationName))
        {
          detailText += $"\n{request.OrganizationName}";
        }

        double rowX = tableX;
        for (int i = 0; i < headers.Length; i++)
        {
          var rect = new XRect(rowX, rowY, actualColWidths[i], rowHeight);
          gfx.DrawRectangle(XPens.Black, rect);
          var format = i == 0 ? XStringFormats.CenterLeft : XStringFormats.CenterRight;
          var text = i == 0 ? detailText : "$0.00";
          if (i == headers.Length - 1)
          {
            gfx.DrawString(text, new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black, rect, format);
          }
          else
          {
            gfx.DrawString(text, normalFont, XBrushes.Black, rect, format);
          }
          rowX += actualColWidths[i];
        }

        // Save PDF to byte array
        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
      }, cancellationToken);

      return ApplicationResult<byte[]>.SuccessResult(pdfBytes, "Invoice generated successfully");
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error generating invoice PDF for patient {PatientId}", request.PatientId);
      return ApplicationResult<byte[]>.Error($"Error generating invoice: {ex.Message}");
    }
  }
}

