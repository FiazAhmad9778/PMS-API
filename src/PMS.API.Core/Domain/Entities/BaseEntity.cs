using PMS.API.Core.Domain.Interfaces;

namespace PMS.API.Core.Domain.Entities;

public abstract class BaseEntity : ISoftDeleteEntity, IAuditEntity
{
  public long Id { get; set; }

  public DateTime CreatedDate { get; set; }

  public long? CreatedBy { get; set; }

  public DateTime? ModifiedDate { get; set; }

  public long? ModifiedBy { get; set; }

  public bool IsDeleted { get; set; }
}

public abstract class BaseEntityWithState : BaseEntity
{
  public bool IsActive { get; set; }
}
