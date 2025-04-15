using System.ComponentModel;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities.Identity;

public class ClaimGroup : BaseEntity, IAggregateRoot
{
  public string? Name { get; set; }
  [DefaultValue(true)]
  public bool IsDisplay { get; set; }
  public virtual List<ApplicationClaim>? ApplicationClaims { get; set; }

  public int Sequence { get; set; }
}
