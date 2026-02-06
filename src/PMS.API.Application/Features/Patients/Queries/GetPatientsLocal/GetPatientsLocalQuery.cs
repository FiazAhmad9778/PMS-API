using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.DTOs.Base;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Patients.Queries.GetPatientsLocal;

public class GetPatientsLocalQuery : PagedQueryBaseRequest, IRequest<ApplicationResult<List<PatientResponseDto>>>
{
}

public class GetPatientsLocalQueryHandler : RequestHandlerBase<GetPatientsLocalQuery, ApplicationResult<List<PatientResponseDto>>>
{
  readonly AppDbContext _appDbContext;

  public GetPatientsLocalQueryHandler(
    IServiceProvider serviceProvider,
    ILogger<GetPatientsLocalQueryHandler> logger,
    AppDbContext appDbContext) : base(serviceProvider, logger)
  {
    _appDbContext = appDbContext;
  }

  protected override async Task<ApplicationResult<List<PatientResponseDto>>> HandleRequest(
    GetPatientsLocalQuery request,
    CancellationToken cancellationToken)
  {
    IQueryable<Patient> query = _appDbContext.Patient
      .AsNoTracking()
      .Where(x => !x.IsDeleted);

    if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
    {
      var keyword = request.SearchKeyword.Trim();
      query = query.Where(x =>
        (x.Name != null && x.Name.Contains(keyword)) ||
        (x.Address != null && x.Address.Contains(keyword)) ||
        (x.DefaultEmail != null && x.DefaultEmail.Contains(keyword)));
    }

    if (!string.IsNullOrWhiteSpace(request.OrderBy))
    {
      var allowedSortColumns = new HashSet<string>
      {
        nameof(Patient.Id),
        nameof(Patient.PatientId),
        nameof(Patient.Name),
        nameof(Patient.Address),
        nameof(Patient.CreatedDate),
        nameof(Patient.ModifiedDate),
        nameof(Patient.Status)
      };

      if (allowedSortColumns.Contains(request.OrderBy))
      {
        if (request.OrderBy == nameof(Patient.Name))
        {
          query = request.SortByAscending
            ? query.OrderBy(x => x.Name.ToLower())
            : query.OrderByDescending(x => x.Name.ToLower());
        }
        else
        {
          query = request.SortByAscending
            ? query.OrderBy(x => EF.Property<object>(x, request.OrderBy))
            : query.OrderByDescending(x => EF.Property<object>(x, request.OrderBy));
        }
      }
    }
    else
    {
      query = query.OrderBy(x => x.Id);
    }

    var totalCount = await query.CountAsync(cancellationToken);

    if (totalCount == 0)
    {
      return ApplicationResult<List<PatientResponseDto>>.SuccessResult(new List<PatientResponseDto>(), 0);
    }

    if (request.PageSize > 0)
    {
      query = query
        .Skip((request.PageNumber - 1) * request.PageSize)
        .Take(request.PageSize);
    }

    var patients = await query
      .Select(x => new PatientResponseDto
      {
        Id = x.Id,
        PatientId = x.PatientId,
        Name = x.Name,
        Address = x.Address,
        DefaultEmail = x.DefaultEmail,
        Status = x.Status,
        CreatedDate = x.CreatedDate,
        InvoicePath = x.InvoiceHistoryList.Where(h => h.PatientId == x.Id)
        .OrderByDescending(h => h.CreatedDate).Select(h => h.FilePath).FirstOrDefault() ?? string.Empty
      })
      .ToListAsync(cancellationToken);

    return ApplicationResult<List<PatientResponseDto>>.SuccessResult(patients, totalCount);
  }
}
