using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommand : IRequest<ApplicationResult<long>>
{
  [Required]
  public required string Name { get; set; }

  public required long PatientId { get; set; }

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

    var patient = new Patient
    {
      Name = request.Name,
      PatientId = request.PatientId, // External ID from Kroll database
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

