namespace PMS.API.Core.Domain.Entities;

public class DocumentMetadata
{
  public long Id { get; set; }
  public long DocumentId { get; set; }
  public string Key { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public Document? Document { get; set; }
}
