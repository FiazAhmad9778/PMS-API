using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Fax.Commands.SendFax;

public class SendFaxCommand : IRequest<ApplicationResult<bool>>
{
  [Required]
  public required string FaxNumber { get; set; }

  public long? DocumentId { get; set; }

  public IFormFile? DocumentFile { get; set; }

  public string? CoverPageMessage { get; set; }
}

public class SendFaxCommandHandler : RequestHandlerBase<SendFaxCommand, ApplicationResult<bool>>
{
  private readonly IDocumentService _documentService;

  public SendFaxCommandHandler(
    IDocumentService documentService,
    IServiceProvider serviceProvider,
    ILogger<SendFaxCommandHandler> logger) : base(serviceProvider, logger)
  {
    _documentService = documentService;
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(SendFaxCommand request, CancellationToken cancellationToken)
  {
    byte[] pdfBytes;
    string? fileName = null;

    // Get PDF bytes from either document ID or uploaded file
    if (request.DocumentId.HasValue && request.DocumentId.Value > 0)
    {
      try
      {
        var fileResult = await _documentService.GetDocumentPdfAsync(request.DocumentId.Value);
        pdfBytes = fileResult.FileContents;
        fileName = fileResult.FileDownloadName;
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, "Error retrieving document {DocumentId}", request.DocumentId);
        return ApplicationResult<bool>.Error($"Document not found: {ex.Message}");
      }
    }
    else if (request.DocumentFile != null)
    {
      using var memoryStream = new MemoryStream();
      await request.DocumentFile.CopyToAsync(memoryStream, cancellationToken);
      pdfBytes = memoryStream.ToArray();
      fileName = request.DocumentFile.FileName;
    }
    else
    {
      return ApplicationResult<bool>.Error("Either DocumentId or DocumentFile must be provided");
    }

    if (pdfBytes == null || pdfBytes.Length == 0)
    {
      return ApplicationResult<bool>.Error("PDF document is empty or invalid");
    }

    //var result = await _faxService.SendFax(
    //  request.FaxNumber,
    //  pdfBytes,
    //  request.CoverPageMessage,
    //  fileName);

    //if (result)
    //{
    //  return ApplicationResult<bool>.SuccessResult(true, "Fax sent successfully");
    //}

    return ApplicationResult<bool>.Error("Failed to send fax");
  }
}

