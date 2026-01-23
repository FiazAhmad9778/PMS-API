using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.DTOs.Base;
using PMS.API.Core.Extensions;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.ARDashboard.Queries.GetARDashboardPatients;

public class GetARDashboardPatientsQuery : PagedQueryBaseRequest, IRequest<ApplicationResult<List<PatientResponseDto>>>
{
}

public class GetARDashboardPatientsQueryHandler : RequestHandlerBase<GetARDashboardPatientsQuery, ApplicationResult<List<PatientResponseDto>>>
{
  private readonly AppDbContext _dbContext;

  public GetARDashboardPatientsQueryHandler(
    AppDbContext dbContext,
    IServiceProvider serviceProvider,
    ILogger<GetARDashboardPatientsQueryHandler> logger) : base(serviceProvider, logger)
  {
    _dbContext = dbContext;
  }

  protected override async Task<ApplicationResult<List<PatientResponseDto>>> HandleRequest(GetARDashboardPatientsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var searchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword.Trim().ToLower() : "";
      
      IQueryable<Patient> patients = _dbContext.Patient.AsNoTracking();

      // Filter by search keyword - search by patient name
      if (!string.IsNullOrEmpty(searchKeyword))
      {
        patients = patients.Where(p => p.Name.ToLower().Contains(searchKeyword));
      }

      // Get total count
      int totalCount = await patients.CountAsync(cancellationToken);

      // Apply sorting and pagination
      IQueryable<Patient> orderedPatients = request.SortByAscending
        ? patients.OrderBy(p => p.CreatedDate)
        : patients.OrderByDescending(p => p.CreatedDate);

      var patientList = await orderedPatients
        .Paginate(request.PageNumber, request.PageSize)
        .ToListAsync(cancellationToken);

      // Map to DTO
      var result = patientList.Select(patient => new PatientResponseDto
      {
        Id = patient.Id,
        PatientId = patient.PatientId,
        Name = patient.Name,
        Address = patient.Address,
        DefaultEmail = patient.DefaultEmail,
        Status = patient.Status ?? "active",
        CreatedDate = patient.CreatedDate
      }).ToList();

      return ApplicationResult<List<PatientResponseDto>>.SuccessResult(result, totalCount);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error fetching patients for AR Dashboard");
      return ApplicationResult<List<PatientResponseDto>>.Error($"Error fetching patients: {ex.Message}");
    }
  }
}

