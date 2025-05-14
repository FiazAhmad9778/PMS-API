using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Documents.DTO;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Core.DTOs.Base;
using PMS.API.Core.Enums;
using PMS.API.Core.Extensions;

namespace PMS.API.Application.Features.Documents.Queries.GetDocuments;

public class GetDocumentsQuery : PagedQueryBaseRequest, IRequest<ApplicationResult<List<DocumentResponseDto>>>
{
  public DocumentStatus Status { get; set; } = DocumentStatus.DataEntryRegTech;
}

public class GetDocumentsQueryHandler : RequestHandlerBase<GetDocumentsQuery, ApplicationResult<List<DocumentResponseDto>>>
{
  private readonly IDocumentRepository _repository;

  public GetDocumentsQueryHandler(
    IDocumentRepository repository,
    IServiceProvider serviceProvider,
    ILogger<GetDocumentsQueryHandler> logger) : base(serviceProvider, logger)
  {
    _repository = repository;
  }

  protected override async Task<ApplicationResult<List<DocumentResponseDto>>> HandleRequest(GetDocumentsQuery request, CancellationToken cancellationToken)
  {
    request.SearchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword!.ToLower() : "";
    var query = _repository.Get().Include(x => x.Metadata).Where(x => x.Status == request.Status).AsQueryable();


    if (!string.IsNullOrEmpty(request.SearchKeyword))
    {
      query = query.Where(d => d.Metadata.Any(m =>
          (m.Key == "UniqueNames" && m.Value.ToLower().Contains(request.SearchKeyword)) ||
          (m.Key == "Cycles" && m.Value.ToLower().Contains(request.SearchKeyword)) ||
          (m.Key == "RXNumbers" && m.Value.ToLower().Contains(request.SearchKeyword))
      ));
    }
    int totalCount = await query.CountAsync();

    // Apply Sorting
    query = query.OrderByDescending(x => x.CreatedDate);

    // Apply Pagination & Projection
    List<DocumentResponseDto> documents = await query
        .Paginate(request.PageNumber, request.PageSize)
        .Select(d => new DocumentResponseDto
        {
          Id = d.Id,
          DocumentUrl = d.DocumentUrl,
          CreatedDate = d.CreatedDate,
          DocumentName = d.DocumentName,
          NoOfPatients = d.NoOfPatients,
          Status = d.Status,
        })
        .ToListAsync();

    return ApplicationResult<List<DocumentResponseDto>>.SuccessResult(documents, totalCount);
  }
}
