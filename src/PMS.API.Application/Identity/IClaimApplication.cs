using System.Security.Claims;
using PMS.API.Core.Domain.Entities.Identity;

namespace PMS.API.Application.Identity;
public interface IClaimApplication
{
  List<ApplicationClaim> GetList();
}
