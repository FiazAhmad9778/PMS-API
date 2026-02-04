using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Invoices.Queries.GetInvoiceDownloadPath;

public class GetInvoiceDownloadPathQuery : IRequest<ApplicationResult<string?>>
{
  public long InvoiceHistoryId { get; set; }
}

public class GetInvoiceDownloadPathQueryHandler : RequestHandlerBase<GetInvoiceDownloadPathQuery, ApplicationResult<string?>>
{
  private readonly AppDbContext _appDbContext;

  public GetInvoiceDownloadPathQueryHandler(
    IServiceProvider serviceProvider,
    ILogger<GetInvoiceDownloadPathQueryHandler> logger,
    AppDbContext appDbContext)
    : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<string?>> HandleRequest(
    GetInvoiceDownloadPathQuery request,
    CancellationToken cancellationToken)
  {
    var filePath = await _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => h.Id == request.InvoiceHistoryId && !h.IsDeleted)
      .Select(h => h.FilePath)
      .FirstOrDefaultAsync(cancellationToken);

    return ApplicationResult<string?>.SuccessResult(string.IsNullOrEmpty(filePath) ? null : filePath);
  }
}
