using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Core.Enums;
using PMS.API.Core.Extensions;

namespace PMS.API.Application.Features.Documents.Queries.GetDocuments;

public class GetPendingDocumentsCountQuery : IRequest<ApplicationResult<int>>
{
}

public class GetPendingDocumentsCountQueryHandler : RequestHandlerBase<GetPendingDocumentsCountQuery, ApplicationResult<int>>
{
  private readonly IDocumentRepository _repository;

  public GetPendingDocumentsCountQueryHandler(
    IDocumentRepository repository,
    IServiceProvider serviceProvider,
    ILogger<GetDocumentsQueryHandler> logger) : base(serviceProvider, logger)
  {
    _repository = repository;
  }

  protected override async Task<ApplicationResult<int>> HandleRequest(GetPendingDocumentsCountQuery request, CancellationToken cancellationToken)
  {
    var query = _repository.Get().Include(x => x.Metadata).Where(x => x.Status == DocumentStatus.Processing).AsQueryable();
    int totalCount = await query.CountAsync();
    return ApplicationResult<int>.SuccessResult(totalCount);
  }
}
