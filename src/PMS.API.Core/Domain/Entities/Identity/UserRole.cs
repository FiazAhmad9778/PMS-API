using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PMS.API.Core.Domain.Entities.Identity;

[Table("UserRole")]
public class UserRole : IdentityUserRole<long>
{
}
