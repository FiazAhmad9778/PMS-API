namespace NumberExtensions;

public static class NumberExtensions
{
  public static bool IsBetween(this int? number, int lowerBound, int upperBound)
  {
    return number != null && number >= lowerBound && number <= upperBound;
  }
}
