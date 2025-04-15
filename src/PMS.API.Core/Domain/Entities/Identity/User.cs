using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Core.Enums;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities.Identity;

[Table("User")]
public class User : IdentityUser<long>, IAggregateRoot, IAuditEntity, ISoftDeleteEntity, IState
{
  public required string FirstName { get; set; }
  public required string LastName { get; set; }
  public string? Address { get; set; }
  public string? AvatarUrl { get; set; }
  public byte[]? SignatureData { get; set; }
  public DateTime CreatedDate { get; set; }
  public long? CreatedBy { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public long? ModifiedBy { get; set; }
  public bool IsDeleted { get; set; }
  public override string? PhoneNumber { get; set; }
  public bool IsActive { get; set; }
  public AppUserTypeEnums UserType { get; set; }
}
