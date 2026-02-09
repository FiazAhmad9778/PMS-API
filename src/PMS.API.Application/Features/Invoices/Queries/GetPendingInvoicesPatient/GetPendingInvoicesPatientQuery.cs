using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Invoices.Queries.GetPendingInvoicesPatient;

/// <summary>
/// Returns patients that have a latest invoice generated (has file) but not sent, with pagination.
/// </summary>
public class GetPendingInvoicesPatientQuery : IRequest<ApplicationResult<List<PatientResponseDto>>>
{
  public int PageNumber { get; set; } = 1;
  public int PageSize { get; set; } = 50;
  public string? SearchKeyword { get; set; }
}

public class GetPendingInvoicesPatientQueryHandler : RequestHandlerBase<GetPendingInvoicesPatientQuery, ApplicationResult<List<PatientResponseDto>>>
{
  private readonly AppDbContext _appDbContext;

  public GetPendingInvoicesPatientQueryHandler(
    IServiceProvider serviceProvider,
    ILogger<GetPendingInvoicesPatientQueryHandler> logger,
    AppDbContext appDbContext)
    : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<List<PatientResponseDto>>> HandleRequest(
    GetPendingInvoicesPatientQuery request,
    CancellationToken cancellationToken)
  {
    // Latest CreatedDate per patient (has file, not deleted)
    var latestPerPatient = _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => !h.IsDeleted && h.PatientId != null && h.FilePath != null && h.FilePath != "")
      .GroupBy(h => h.PatientId!.Value)
      .Select(g => new { PatientId = g.Key, MaxCreated = g.Max(h => h.CreatedDate) });

    // Patient IDs where that latest invoice is not sent
    var pendingPatientIds = await _appDbContext.InvoiceHistory
      .AsNoTracking()
      .Where(h => !h.IsDeleted && h.PatientId != null && !h.IsSent && h.FilePath != null && h.FilePath != "")
      .Where(h => latestPerPatient.Any(l => l.PatientId == h.PatientId!.Value && l.MaxCreated == h.CreatedDate))
      .Select(h => h.PatientId!.Value)
      .Distinct()
      .ToListAsync(cancellationToken);

    if (pendingPatientIds.Count == 0)
      return ApplicationResult<List<PatientResponseDto>>.SuccessResult(new List<PatientResponseDto>(), 0);

    var patientQuery = _appDbContext.Patient
      .AsNoTracking()
      .Where(p => !p.IsDeleted && pendingPatientIds.Contains(p.Id));

    if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
    {
      var kw = request.SearchKeyword.Trim();
      patientQuery = patientQuery.Where(p =>
        (p.Name != null && p.Name.Contains(kw)) ||
        (p.Address != null && p.Address.Contains(kw)) ||
        (p.DefaultEmail != null && p.DefaultEmail.Contains(kw)));
    }

    var totalCount = await patientQuery.CountAsync(cancellationToken);

    var patients = await patientQuery
      .OrderBy(p => p.Name)
      .Skip((request.PageNumber - 1) * request.PageSize)
      .Take(request.PageSize)
      .Select(p => new PatientResponseDto
      {
        Id = p.Id,
        PatientId = p.PatientId,
        Name = p.Name,
        Address = p.Address,
        DefaultEmail = p.DefaultEmail,
        Status = p.Status,
        CreatedDate = p.CreatedDate,
        InvoicePath = p.InvoiceHistoryList
          .Where(h => h.PatientId == p.Id)
          .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
          .Select(h => h.FilePath)
          .FirstOrDefault() ?? string.Empty,
        InvoiceFromDate = p.InvoiceHistoryList
          .Where(h => h.PatientId == p.Id)
          .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
          .Select(h => (DateTime?)h.InvoiceStartDate)
          .FirstOrDefault(),
        InvoiceToDate = p.InvoiceHistoryList
          .Where(h => h.PatientId == p.Id)
          .OrderByDescending(h => h.CreatedDate).ThenByDescending(h => h.Id)
          .Select(h => (DateTime?)h.InvoiceEndDate)
          .FirstOrDefault(),
        InvoiceIsSent = false
      })
      .ToListAsync(cancellationToken);

    return ApplicationResult<List<PatientResponseDto>>.SuccessResult(patients, totalCount);
  }
}
