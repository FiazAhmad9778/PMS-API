using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Documents.Commands.UploadSignedDocument;
using PMS.API.Application.Features.Documents.DTO;
using PMS.API.Application.Features.Documents.Queries.GetDocuments;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Web.Api;
[Route("api/[controller]")]
[Authorize]
[ApiController]
public class DocumentController : BaseApiController
{
  private readonly IDocumentService _documentService;

  public DocumentController(IServiceProvider serviceProvider, IDocumentService documentService) : base(serviceProvider)
  {
    _documentService = documentService;
  }

  [HttpGet("list")]
  [ProducesResponseType(typeof(ApplicationResult<List<DocumentResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<DocumentResponseDto>>> GetDocuments([FromQuery] GetDocumentsQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpGet("view")]
  [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
  public async Task<FileContentResult> GetDocumentForViewAsync([FromQuery] long id)
  {
    return await _documentService.GetDocumentPdfAsync(id);
  }

  [HttpGet("get-pending-document-count")]
  [ProducesResponseType(typeof(ApplicationResult<int>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<int>> GetPendingDocumentCount([FromQuery] GetPendingDocumentsCountQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpPost("upload-signed-document")]
  [ProducesResponseType(typeof(ApplicationResult<bool>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<bool>> UploadSignedDocument([FromForm] UploadSignedDocumentCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("upload-completed-document")]
  [ProducesResponseType(typeof(ApplicationResult<bool>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<bool>> UploadSignedDocument([FromForm] UploadCompletedDocumentCommand command)
  {
    return await Mediator.Send(command);
  }
}
