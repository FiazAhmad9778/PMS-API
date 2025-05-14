using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Documents.DTO;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Core.Enums;
using PMS.API.Core.Extensions;

namespace PMS.API.Application.Features.Documents.Queries.GetDocuments;

public class GetPendingDocumentsCountQuery : IRequest<ApplicationResult<PendingDocumentResponseDto>>
{
}

public class GetPendingDocumentsCountQueryHandler : RequestHandlerBase<GetPendingDocumentsCountQuery, ApplicationResult<PendingDocumentResponseDto>>
{
  private readonly IDocumentRepository _repository;

  public GetPendingDocumentsCountQueryHandler(
    IDocumentRepository repository,
    IServiceProvider serviceProvider,
    ILogger<GetDocumentsQueryHandler> logger) : base(serviceProvider, logger)
  {
    _repository = repository;
  }

  protected override async Task<ApplicationResult<PendingDocumentResponseDto>> HandleRequest(GetPendingDocumentsCountQuery request, CancellationToken cancellationToken)
  {
    var dataEntryRegTech = _repository.Get().Include(x => x.Metadata).Where(x => x.Status == DocumentStatus.DataEntryRegTech).AsQueryable();
    int regTechCount = await dataEntryRegTech.CountAsync();
    var physicalCheck = _repository.Get().Include(x => x.Metadata).Where(x => x.Status == DocumentStatus.PhysicalCheckRegTech).AsQueryable();
    int physicalCheckCount = await physicalCheck.CountAsync();
    return ApplicationResult<PendingDocumentResponseDto>.SuccessResult(new PendingDocumentResponseDto { DataEntryCount = regTechCount, PhysicalCheckCount = physicalCheckCount });
  }
}
