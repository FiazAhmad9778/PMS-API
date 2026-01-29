namespace PMS.API.Application.Common.ConectionStringHelper;

public static class ConnectionStringHelper
{
  public static string ExtractDatabaseName(string connectionString)
  {
    var dbIndex = connectionString.IndexOf("Database=", StringComparison.OrdinalIgnoreCase);
    if (dbIndex == -1) return "Kroll"; // Fallback to default

    var startIndex = dbIndex + "Database=".Length;
    var endIndex = connectionString.IndexOf(";", startIndex);
    if (endIndex == -1) endIndex = connectionString.Length;

    return connectionString.Substring(startIndex, endIndex - startIndex).Trim();
  }
}
