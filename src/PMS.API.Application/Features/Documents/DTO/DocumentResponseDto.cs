using PMS.API.Core.Enums;

namespace PMS.API.Application.Features.Documents.DTO;
public class DocumentResponseDto
{
  public long Id { get; set; }
  public required string DocumentName { get; set; }
  public required string DocumentUrl { get; set; }
  public DateTime CreatedDate { get; set; }
  public int NoOfPatients { get; set; }
  public DocumentStatus Status { get; set; }
}


public class PendingDocumentResponseDto
{
  public int DataEntryCount { get; set; }
  public int PhysicalCheckCount { get; set; }
}
