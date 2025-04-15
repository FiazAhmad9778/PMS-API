namespace PMS.API.SharedKernel.Constants_and_Enums.DateTime;

public class DateTimeConstants
{
  public static string[] fullMonthNames = {
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
};

  public static string[] abbreviatedMonthNames = {
    "Jan", "Feb", "Mar", "Apr", "May", "Jun",
    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
};
  public static string[] hoursInAmPmFormat =
  {
    "6AM", "7AM", "8AM", "9AM", "10AM", "11AM", "12PM", "1PM", "2PM", "3PM", "4PM", "5PM",
    "6PM", "7PM", "8PM", "9PM", "10PM", "11PM", "12AM", "1AM", "2AM", "3AM", "4AM", "5AM"
};
  public enum AbbreviatedMonthNames
  {
    Jan = 1, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec
  }
}
