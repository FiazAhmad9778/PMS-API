using System.ComponentModel.DataAnnotations.Schema;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities;

[Table("OrganizationPatient")]
public class OrganizationPatient : IAggregateRoot
{
  public long Id { get; set; }
  public required string Name { get; set; }
  public required long ExternalId { get; set; }
  public long? OrganizationId { get; set; }
  public Organization? Organization { get; set; }
  public long? WardId { get; set; }
  public Ward? Ward { get; set; }

  public List<InvoiceHistory> InvoiceHistoryList { get; set; } = new List<InvoiceHistory>();

}
