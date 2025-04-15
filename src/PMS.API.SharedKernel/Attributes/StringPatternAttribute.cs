using System.Text.RegularExpressions;

namespace PMS.API.SharedKernel.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class StringPatternAttribute : Attribute
{
  public bool AllowNull { get; set; }
  public bool AllowEmpty { get; set; }
  public string? RegexPattern { get; set; }

  public StringPatternAttribute(bool allowNull = true, bool allowEmpty = true, string? regexPattern = null)
  {
    AllowNull = allowNull;
    AllowEmpty = allowEmpty;
    RegexPattern = regexPattern;
  }

  public bool IsValid(string? value)
  {
    if (value == null)
    {
      return AllowNull;
    }

    if (value == string.Empty)
    {
      return AllowEmpty;
    }

    if (RegexPattern != null)
    {
      return Regex.IsMatch(value, RegexPattern);
    }

    return true;
  }
}
