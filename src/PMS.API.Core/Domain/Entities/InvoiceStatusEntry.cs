namespace PMS.API.Core.Domain.Entities;

public class InvoiceStatusEntry
{
  public DateTime Timestamp { get; set; }
  public string Status { get; set; } = string.Empty;
}
