using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities.Identity;

// old name RoleClaim
public class ApplicationClaim : ISoftDeleteEntity, IAggregateRoot
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }
  public string? ClaimCode { get; set; }
  public string? ClaimValue { get; set; }
  public bool IsDeleted { get; set; }

  [DefaultValue(true)]
  public bool IsDisplay { get; set; }

  [ForeignKey("ClaimGroup")]
  public long ClaimGroupId { get; set; }
  public virtual ClaimGroup? ClaimGroup { get; set; }
  public bool IsAllowedToAll { get; set; }
  public List<int>? AllowedSubscriptions { get; set; }
}
