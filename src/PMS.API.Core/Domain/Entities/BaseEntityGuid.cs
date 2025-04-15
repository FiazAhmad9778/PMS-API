using PMS.API.Core.Domain.Interfaces;

namespace PMS.API.Core.Domain.Entities;

public abstract class BaseEntityGuid : ISoftDeleteEntity, IAuditEntity
{
  public Guid Id { get; set; }

  public DateTime CreatedDate { get; set; }

  public long? CreatedBy { get; set; }

  public DateTime? ModifiedDate { get; set; }

  public long? ModifiedBy { get; set; }

  public bool IsDeleted { get; set; }
}

