namespace PMS.API.SharedKernel.Extensions;
public static class DateTimeExtensions
{
  public static DateTime UpdateDateToToday(this DateTime originalDateTime)
  {
    DateTime today = DateTime.Today;
    TimeSpan timeOfDay = originalDateTime.TimeOfDay;

    return today + timeOfDay;
  }

  public static DateTime? UpdateDateToToday(this DateTime? originalDateTime)
  {
    if (originalDateTime.HasValue)
    {
      DateTime today = DateTime.Today;
      TimeSpan timeOfDay = originalDateTime.Value.TimeOfDay;

      return today + timeOfDay;
    }
    return null;
  }
}
