namespace PMS.API.Core.Domain.Interfaces;

public interface ISoftDeleteEntity
{
  public bool IsDeleted { get; set; }
}
