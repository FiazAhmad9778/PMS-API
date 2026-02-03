using System.Text.Json;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Application.Common.Helpers;

public static class InvoiceStatusHistoryHelper
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
  };

  /// <summary>
  /// Appends a status entry to the JSON history and sets the current status.
  /// </summary>
  public static void AppendStatus(InvoiceHistory record, string status)
  {
    var now = DateTime.UtcNow;
    var entry = new InvoiceStatusEntry { Timestamp = now, Status = status };
    var list = Deserialize(record.InvoiceStatusHistory);
    list.Add(entry);
    record.InvoiceStatusHistory = Serialize(list);
    record.InvoiceStatus = status;
    record.ModifiedDate = now;
  }

  public static List<InvoiceStatusEntry> Deserialize(string? json)
  {
    if (string.IsNullOrWhiteSpace(json))
      return new List<InvoiceStatusEntry>();
    try
    {
      var list = JsonSerializer.Deserialize<List<InvoiceStatusEntry>>(json, JsonOptions);
      return list ?? new List<InvoiceStatusEntry>();
    }
    catch
    {
      return new List<InvoiceStatusEntry>();
    }
  }

  public static string Serialize(List<InvoiceStatusEntry> list)
  {
    return JsonSerializer.Serialize(list, JsonOptions);
  }
}
