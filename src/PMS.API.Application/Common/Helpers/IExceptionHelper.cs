using FirebaseAdmin.Auth.Multitenancy;
using PMS.API.Application.Common.Context.Models;
using PMS.API.Application.DTOs.Common.Base.Response;

namespace PMS.API.Application.Common.Helpers;

public interface IExceptionHelper
{
  Task<ErrorResponse> GetErrorResponse(Exception ex, CurrentUserContext currentUser, bool sendEmail = false, string? customMessage = null, bool logToDb = true);
  Task<long> Log(Exception e, CurrentUserContext currentUser, bool sendEmail = false);
}
