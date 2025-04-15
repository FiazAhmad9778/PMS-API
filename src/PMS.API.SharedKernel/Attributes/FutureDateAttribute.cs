using System.ComponentModel.DataAnnotations;

namespace PMS.API.SharedKernel.Attributes;
[AttributeUsage(AttributeTargets.Property)]
public class FutureDateAttribute : ValidationAttribute
{
  public bool AllowToday { get; }
  public bool IgnoreTime { get; }

  public FutureDateAttribute(bool allowToday = false, bool ignoreTime = false)
  {
    AllowToday = allowToday;
    IgnoreTime = ignoreTime;
  }

  public override bool IsValid(object? value)
  {
    if (value == null)
    {
      return true; // Null value is considered valid for nullable properties
    }


    if (value is DateTime dateTime)
    {
      var currentDate = DateTime.Now;
      if (IgnoreTime)
      {
        currentDate = currentDate.Date;
        dateTime = dateTime.Date;
      }

      if (AllowToday)
      {
        return dateTime >= currentDate;
      }
      else
      {
        return dateTime > currentDate;
      }
    }

    // Non-DateTime properties are considered valid
    return true;
  }
}
