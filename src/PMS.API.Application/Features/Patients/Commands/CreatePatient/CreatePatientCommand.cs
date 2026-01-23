using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.DTOs.Common.Base.Response;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Infrastructure.Data;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommand : IRequest<ApplicationResult<long>>
{
  [Required]
  public required string Name { get; set; }

  public string? PatientId { get; set; }

  public string? Address { get; set; }

  public string? DefaultEmail { get; set; }

  public string Status { get; set; } = "active";
}

public class CreatePatientHandler : RequestHandlerBase<CreatePatientCommand, ApplicationResult<long>>
{
  private readonly SharedKernel.Interfaces.IRepository<Patient> _patientRepository;
  private readonly IEfRepository<Organization> _organizationRepository;
  private readonly AppDbContext _dbContext;

  public CreatePatientHandler(
    SharedKernel.Interfaces.IRepository<Patient> patientRepository,
    IEfRepository<Organization> organizationRepository,
    AppDbContext dbContext,
    IServiceProvider serviceProvider,
    ILogger<CreatePatientHandler> logger) : base(serviceProvider, logger)
  {
    _patientRepository = patientRepository;
    _organizationRepository = organizationRepository;
    _dbContext = dbContext;
  }

  protected override async Task<ApplicationResult<long>> HandleRequest(CreatePatientCommand request, CancellationToken cancellationToken)
  {
    // Use organization data directly from the request payload
    // No need to look up organization from database since the data is already in the payload

    // Generate PatientId if not provided
    var patientId = request.PatientId;
    if (string.IsNullOrWhiteSpace(patientId))
    {
      // Auto-generate PatientId using timestamp format: PAT-YYYYMMDDHHMMSS
      patientId = $"PAT-{DateTime.UtcNow:yyyyMMddHHmmss}";
      
      // Ensure uniqueness by checking if it exists using DbContext directly
      var existingPatient = await _dbContext.Patient
        .FirstOrDefaultAsync(p => p.PatientId == patientId, cancellationToken);
      if (existingPatient != null)
      {
        // If exists, append a random suffix
        patientId = $"PAT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";
      }
    }

    var patient = new Patient
    {
      Name = request.Name,
      PatientId = patientId, // External ID from Kroll database (ARID)
      Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
      DefaultEmail = string.IsNullOrWhiteSpace(request.DefaultEmail) ? null : request.DefaultEmail,
      Status = request.Status ?? "active",
      CreatedDate = DateTime.UtcNow,
      IsDeleted = false
    };

    await _patientRepository.AddAsync(patient);
    await _dbContext.SaveChangesAsync(cancellationToken);

    return ApplicationResult<long>.SuccessResult(patient.Id);
  }
}

