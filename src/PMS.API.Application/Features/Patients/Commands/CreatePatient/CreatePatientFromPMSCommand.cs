using System.Data;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.ConectionStringHelper;
using PMS.API.Application.Common.Models;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Features.Patients.Commands.CreatePatient;
public class CreatePatientFromPMSCommand : IRequest<ApplicationResult<bool>>
{
  public required long[] PatientId { get; set; }
}


public class CreatePatientFromPMSCommandHandler : RequestHandlerBase<CreatePatientFromPMSCommand, ApplicationResult<bool>>
{
  readonly SharedKernel.Interfaces.IRepository<Patient> _patientRepository;
  readonly IEfRepository<Organization> _organizationRepository;
  readonly IConfiguration _configuration;
  readonly AppDbContext _dbContext;
  readonly string _connectionString;
  readonly string _databaseName;

  public CreatePatientFromPMSCommandHandler(
    SharedKernel.Interfaces.IRepository<Patient> patientRepository,
    IEfRepository<Organization> organizationRepository,
    AppDbContext dbContext,
    IServiceProvider serviceProvider,
    ILogger<CreatePatientHandler> logger,
    IConfiguration configuration) : base(serviceProvider, logger)
  {
    _configuration = configuration;
    _patientRepository = patientRepository;
    _organizationRepository = organizationRepository;
    _dbContext = dbContext;
    _connectionString = _configuration.GetConnectionString("ARDashboardConnection")
     ?? throw new InvalidOperationException("Connection string 'ARDashboardConnection' not found.");

    _databaseName = ConnectionStringHelper.ExtractDatabaseName(_connectionString);
  }
  protected override async Task<ApplicationResult<bool>> HandleRequest(
      CreatePatientFromPMSCommand request,
      CancellationToken cancellationToken)
  {
    if (request.PatientId == null || !request.PatientId.Any())
      return ApplicationResult<bool>.Error("PatientIds are required.");

    var existingPatientIds = await _dbContext.Patient
        .Where(p => request.PatientId.Contains(p.PatientId))
        .Select(p => p.PatientId)
        .ToListAsync(cancellationToken);

    var missingPatientIds = request.PatientId
        .Except(existingPatientIds)
        .ToArray();

    if (!missingPatientIds.Any())
    {
      return ApplicationResult<bool>.Error("patients Already Exists.");
    }

    var patientsToInsert = new List<Patient>();

    using (var connection = new SqlConnection(_connectionString))
    {
      await connection.OpenAsync(cancellationToken);

      var query = $@"
        SELECT
            ID AS PatientId,
            FirstName + ' ' + LastName AS PatientName,
            Address1 AS PatientAddress,
            EMail AS PatientEmail,
            CreatedOn AS PatientCreatedDate
        FROM dbo.Pat
        WHERE ID IN ({string.Join(",", missingPatientIds)})";

      using var command = new SqlCommand(query, connection);

      using var reader = await command.ExecuteReaderAsync(cancellationToken);

      while (await reader.ReadAsync(cancellationToken))
      {
        patientsToInsert.Add(new Patient
        {
          PatientId = Convert.ToInt64(reader[0]),           
          Name = reader.IsDBNull(1) ? "" : reader.GetString(1),   
          Address = reader.IsDBNull(2) ? "" : reader.GetString(2),
          DefaultEmail = reader.IsDBNull(3) ? "" : reader.GetString(3),
          CreatedDate = reader.IsDBNull(4) ? DateTime.UtcNow : reader.GetDateTime(4),
          Status = "active",
          IsDeleted = false
        });
      }
    }

    if (!patientsToInsert.Any())
    {
      return ApplicationResult<bool>.Error("No patients found in PMS.");
    }

    await _dbContext.Patient.AddRangeAsync(patientsToInsert, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);

    return ApplicationResult<bool>.SuccessResult(true);
  }

}
