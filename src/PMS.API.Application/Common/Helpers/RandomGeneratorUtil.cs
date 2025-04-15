namespace PMS.API.Application.Common.Helpers;
using System;
using System.Text;

public class RandomGeneratorUtil
{
  private static readonly Random random = new Random();
  private static long lastTimestamp = 0;
  private static long counter = 0;
  private static readonly object lockObject = new object();

  // Define character sets for password generation
  private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
  private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
  private const string Digits = "0123456789";
  private const string SpecialCharacters = "!@#$%&()-_[]{}|<>?";

  public static string GenerateRandomPassword(int length = 12)
  {
    // Make sure each policy is met by picking at least one character from each set
    var passwordChars = new StringBuilder();
    // Fill the rest of the password with random characters
    while (passwordChars.Length < length)
    {
      passwordChars.Append(GetRandomChar(LowercaseLetters));
      passwordChars.Append(GetRandomChar(UppercaseLetters));
      passwordChars.Append(GetRandomChar(Digits));
      passwordChars.Append(GetRandomChar(SpecialCharacters));
    }

    // Shuffle the characters to ensure randomness
    return ShuffleString(passwordChars.ToString());
  }

  private static char GetRandomChar(string charSet)
  {
    int index = random.Next(charSet.Length);
    return charSet[index];
  }

  private static string ShuffleString(string input)
  {
    char[] chars = input.ToCharArray();
    for (int i = chars.Length - 1; i > 0; i--)
    {
      int j = random.Next(i + 1);
      char temp = chars[i];
      chars[i] = chars[j];
      chars[j] = temp;
    }
    return new string(chars);
  }

  public static long GenerateUnique10DigitNumber()
  {
    lock (lockObject)
    {
      long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

      // If the timestamp is the same as the last one, increment the counter to ensure uniqueness
      if (timestamp == lastTimestamp)
      {
        counter++;
      }
      else
      {
        // Reset the counter if the timestamp has changed
        counter = 0;
        lastTimestamp = timestamp;
      }

      // Combine the timestamp with the counter to generate a unique number
      long uniqueNumber = (timestamp * 100 + counter) % 10000000000L;

      return uniqueNumber;
    }
  }
}

