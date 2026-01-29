using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Application.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommand : IRequest<ApplicationResult<long>>
{
  [Required]
  public required string Name { get; set; }
  public required long OrganizationExternalId { get; set; }

  public long[]? WardIds { get; set; }

  public string? Address { get; set; }

  public string? DefaultEmail { get; set; }
}

public class CreateOrganizationHandler : RequestHandlerBase<CreateOrganizationCommand, ApplicationResult<long>>
{
  private readonly SharedKernel.Interfaces.IRepository<Organization> _repository;
  private readonly AppDbContext _dbContext;

  public CreateOrganizationHandler(
    SharedKernel.Interfaces.IRepository<Organization> repository,
    AppDbContext dbContext,
    IServiceProvider serviceProvider,
    ILogger<CreateOrganizationHandler> logger) : base(serviceProvider, logger)
  {
    _repository = repository;
    _dbContext = dbContext;
  }

  protected override async Task<ApplicationResult<long>> HandleRequest(CreateOrganizationCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Check if organization already exists in PostgreSQL
      var existingOrganization = await _dbContext.Organization
        .FirstOrDefaultAsync(o => o.Name == request.Name && !o.IsDeleted, cancellationToken);

      long organizationId;
      
      if (existingOrganization == null)
      {

        // Organization doesn't exist, create it
        var organization = new Organization
        {
          Name = request.Name,
          OrganizationExternalId=request.OrganizationExternalId,
          Address = string.IsNullOrWhiteSpace(request.Address) ? string.Empty : request.Address,
          DefaultEmail = string.IsNullOrWhiteSpace(request.DefaultEmail) ? null : request.DefaultEmail,
          CreatedDate = DateTime.UtcNow,
          IsDeleted = false
        };

        await _repository.AddAsync(organization);
        await _dbContext.SaveChangesAsync(cancellationToken);
        organizationId = organization.Id;

        // Load and assign wards if WardIds are provided
        if (request.WardIds != null && request.WardIds.Length > 0)
        {
          var wards = await _dbContext.Ward
            .Where(w => request.WardIds.Contains(w.Id) && !w.IsDeleted)
            .ToListAsync(cancellationToken);
          
          foreach (var ward in wards)
          {
            ward.OrganizationId = organizationId; // Set the foreign key
          }
          
          await _dbContext.SaveChangesAsync(cancellationToken);
        }
        Logger.LogInformation($"Created new organization '{request.Name}' with ID {organizationId}");
      }
      else
      {
        organizationId = existingOrganization.Id;
        Logger.LogInformation($"Organization '{request.Name}' already exists with ID {organizationId}");
      }

      return ApplicationResult<long>.SuccessResult(organizationId);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, $"Error creating organization '{request.Name}': {ex.Message}");
      return ApplicationResult<long>.Error($"Error creating organization: {ex.Message}");
    }
  }
}

