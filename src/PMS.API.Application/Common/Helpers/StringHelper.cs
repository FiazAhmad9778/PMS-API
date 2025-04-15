namespace PMS.API.Application.Common.Helpers;
public static class StringHelper
{
  private static string prefix = "CC";
  private static string OrderPrefix = "OR";
  private static string RoutePrefix = "ROU";
  private static string SupportTicketPrefix = "ST";

  public static string GenerateCustomerID(string? lastCustomerID)
  {
    long lastIDNumber = 1;

    if (!string.IsNullOrEmpty(lastCustomerID))
    {
      if (lastCustomerID.StartsWith(prefix) && lastCustomerID.Length >= prefix.Length + 1)
      {
        string numericPart = lastCustomerID.Substring(prefix.Length); // Extract numeric part after prefix
        if (long.TryParse(numericPart, out lastIDNumber))
        {
          lastIDNumber++; // Increment the last number
        }
      }
    }

    string format = $"{prefix}{lastIDNumber.ToString("D5")}"; // Use "D5" format to maintain leading zeros
    return format;
  }

  public static string GenerateSupportTicketID(string? lastTicketId)
  {
    long lastIDNumber = 1;

    if (!string.IsNullOrEmpty(lastTicketId))
    {
      if (lastTicketId.StartsWith(SupportTicketPrefix) && lastTicketId.Length >= SupportTicketPrefix.Length + 1)
      {
        string numericPart = lastTicketId.Substring(SupportTicketPrefix.Length); // Extract numeric part after prefix
        if (long.TryParse(numericPart, out lastIDNumber))
        {
          lastIDNumber++; // Increment the last number
        }
      }
    }

    string format = $"{SupportTicketPrefix}{lastIDNumber.ToString("D5")}"; // Use "D5" format to maintain leading zeros
    return format;
  }

  public static string GenerateOrderID(string? lastOrderId)
  {
    long lastIDNumber = 1;

    if (!string.IsNullOrEmpty(lastOrderId))
    {
      if (lastOrderId.StartsWith(OrderPrefix) && lastOrderId.Length >= OrderPrefix.Length + 1)
      {
        string numericPart = lastOrderId.Substring(OrderPrefix.Length); // Extract numeric part after prefix
        if (long.TryParse(numericPart, out lastIDNumber))
        {
          lastIDNumber++; // Increment the last number
        }
      }
    }

    string format = $"{OrderPrefix}{lastIDNumber.ToString("D8")}"; // Use "D8" format to maintain leading zeros
    return format;
  }

  public static string GenerateRouteID(string? lastOrderId)
  {
    long lastIDNumber = 1;

    if (!string.IsNullOrEmpty(lastOrderId))
    {
      if (lastOrderId.StartsWith(RoutePrefix) && lastOrderId.Length >= RoutePrefix.Length + 1)
      {
        string numericPart = lastOrderId.Substring(RoutePrefix.Length); // Extract numeric part after prefix
        if (long.TryParse(numericPart, out lastIDNumber))
        {
          lastIDNumber++; // Increment the last number
        }
      }
    }

    string format = $"{RoutePrefix}{lastIDNumber.ToString("D8")}"; // Use "D8" format to maintain leading zeros
    return format;
  }
}
