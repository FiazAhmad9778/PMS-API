using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Documents.Commands.UploadSignedDocument;

public class UploadCompletedDocumentCommand : IRequest<ApplicationResult<bool>>
{
  public required IFormFile CompletedDocument { get; set; }
}

public class UploadCompletedDocumentCommandHandler : RequestHandlerBase<UploadCompletedDocumentCommand, ApplicationResult<bool>>
{
  private readonly IDocumentService _documentService;

  public UploadCompletedDocumentCommandHandler(
    IDocumentService documentService,
    IServiceProvider serviceProvider,
    ILogger<UploadCompletedDocumentCommandHandler> logger) : base(serviceProvider, logger)
  {
    _documentService = documentService;
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(UploadCompletedDocumentCommand request, CancellationToken cancellationToken)
  {
    return await _documentService.UploadDocumentToCompleted(request.CompletedDocument);
  }
}
