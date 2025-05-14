using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Documents.DTO;
using PMS.API.Core.Domain.Interfaces.Repositories;

namespace PMS.API.Application.Features.Documents.Queries.GetDocument;

public class GetDocumentQuery : IRequest<ApplicationResult<DocumentResponseDto>>
{
  public long Id { get; set; }
}

public class GetDocumentsQueryHandler : RequestHandlerBase<GetDocumentQuery, ApplicationResult<DocumentResponseDto>>
{
  private readonly IDocumentRepository _repository;

  public GetDocumentsQueryHandler(
    IDocumentRepository repository,
    IServiceProvider serviceProvider,
    ILogger<GetDocumentsQueryHandler> logger) : base(serviceProvider, logger)
  {
    _repository = repository;
  }

  protected override async Task<ApplicationResult<DocumentResponseDto>> HandleRequest(GetDocumentQuery request, CancellationToken cancellationToken)
  {
    var document = await _repository.Get().FirstOrDefaultAsync(d => d.Id == request.Id);

    if (document == null) return ApplicationResult<DocumentResponseDto>.Error("Docuemnt not found!");

    var documentResult = new DocumentResponseDto { DocumentName = document.DocumentName, DocumentUrl = document.DocumentUrl, CreatedDate = document.CreatedDate, Id = document.Id, NoOfPatients = document.NoOfPatients, Status = document.Status };

    return ApplicationResult<DocumentResponseDto>.SuccessResult(documentResult);
  }
}
