namespace PMS.API.Core.Domain.Interfaces;

public interface IAuditEntity
{
  public DateTime CreatedDate { get; set; }

  public long? CreatedBy { get; set; }

  public DateTime? ModifiedDate { get; set; }

  public long? ModifiedBy { get; set; }
}
