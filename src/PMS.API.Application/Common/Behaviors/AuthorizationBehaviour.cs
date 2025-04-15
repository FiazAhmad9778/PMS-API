using System.Reflection;
using MediatR;
using PMS.API.Application.Common.Exceptions;
using PMS.API.Application.Common.Security;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Infrastructure.Interfaces;

namespace PMS.API.Application.Common.Behaviors;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  private readonly ICurrentUserService _currentUserService;
  private readonly IIdentityService _identityService;

  public AuthorizationBehaviour(
      ICurrentUserService currentUserService,
      IIdentityService identityService)
  {
    _currentUserService = currentUserService;
    _identityService = identityService;
  }

  public async Task<TResponse> Handle(TRequest request,
      RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeRequestAttribute>().ToList();

    if (authorizeAttributes.Any())
    {
      // Must be authenticated user
      if (!_currentUserService.UserId.HasValue)
      {
        throw new UnauthorizedAccessException();
      }

      // Role-based authorization
      var authorizeAttributesWithRoles =
          authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles)).ToList();

      if (authorizeAttributesWithRoles.Any())
      {
        var authorized = false;

        foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
        {
          foreach (var role in roles)
          {
            var isInRole =
                await _identityService.IsInRoleAsync(_currentUserService.UserId!.Value, role.Trim());
            if (isInRole)
            {
              authorized = true;
              break;
            }
          }
        }

        // Must be a member of at least one role in roles
        if (!authorized)
        {
          throw new ForbiddenAccessException();
        }
      }

      // Policy-based authorization
      var authorizeAttributesWithPolicies =
          authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy)).ToList();

      if (authorizeAttributesWithPolicies.Any())
      {
        foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
        {
          var authorized = await _identityService.AuthorizeAsync(_currentUserService.UserId!.Value, policy);

          if (!authorized)
          {
            throw new ForbiddenAccessException();
          }
        }
      }
    }
    return await next();
  }
}
