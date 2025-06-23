using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Documents.Commands.UploadSignedDocument;
public class UploadSignedDocumentCommand : IRequest<ApplicationResult<bool>>
{
  public long Id { get; set; }
  public required IFormFile SignedDocument { get; set; }
}

public class UploadSignedDocumentCommandHandler : RequestHandlerBase<UploadSignedDocumentCommand, ApplicationResult<bool>>
{
  private readonly IDocumentService _documentService;

  public UploadSignedDocumentCommandHandler(
    IDocumentService documentService,
    IServiceProvider serviceProvider,
    ILogger<UploadSignedDocumentCommandHandler> logger) : base(serviceProvider, logger)
  {
    _documentService = documentService;
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(UploadSignedDocumentCommand request, CancellationToken cancellationToken)
  {
    return await _documentService.UploadSignedDocument(request.Id, request.SignedDocument);
  }
}


public class DeleteUnSignedDocumentCommand : IRequest<ApplicationResult<bool>>
{
  public long Id { get; set; }
}

public class DeleteUnSignedDocumentCommandHandler : RequestHandlerBase<DeleteUnSignedDocumentCommand, ApplicationResult<bool>>
{
  private readonly IDocumentService _documentService;

  public DeleteUnSignedDocumentCommandHandler(
    IDocumentService documentService,
    IServiceProvider serviceProvider,
    ILogger<UploadSignedDocumentCommandHandler> logger) : base(serviceProvider, logger)
  {
    _documentService = documentService;
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(DeleteUnSignedDocumentCommand request, CancellationToken cancellationToken)
  {
    return await _documentService.DeleteUnSignedDocument(request.Id);
  }
}

