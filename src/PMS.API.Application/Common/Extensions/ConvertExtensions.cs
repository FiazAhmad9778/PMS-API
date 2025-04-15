using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.API.Application.Common.Extensions;

/// <summary>
/// ConvertExtensions contains extensions for data convertion for premitive types.
/// </summary>
public static class ConvertExtensions
{

  /// <summary>
  /// GetIntegerValue Converts the sourceValue to int.
  /// </summary>
  /// <param name="sourceValue"></param>
  /// <param name="defaultValue"></param>
  /// <returns></returns>
  public static int GetIntegerValue(this object sourceValue, int defaultValue = default)
  {
    var value = sourceValue.GetIntegerValueNullable();

    if (value.HasValue)
    {
      return value.Value;
    }

    return defaultValue;
  }

  /// <summary>
  /// GetIntegerValueNullable Converts the sourceValue to Nullable<int>.
  /// </summary>
  /// <param name="sourceValue"></param>
  /// <returns></returns>
  public static int? GetIntegerValueNullable(this object sourceValue)
  {
    int? value = null;

    if (sourceValue != null)
    {
      if (sourceValue is int)
      {
        value = (int)sourceValue;
      }
      else if (!string.IsNullOrWhiteSpace(sourceValue.ToString()))
      {
        value = Convert.ToInt32(sourceValue);
      }
    }

    return value;
  }

  /// <summary>
  /// GetByteValue Converts the sourceValue to byte.
  /// </summary>
  /// <param name="sourceValue"></param>
  /// <returns></returns>
  public static byte GetByteValue(this object sourceValue)
  {
    var value = sourceValue.GetByteValueNullable();

    if (value.HasValue)
    {
      return value.Value;
    }

    return default;
  }

  /// <summary>
  /// GetByteValueNullable Converts the sourceValue to Nullable<byte/>.
  /// </summary>
  /// <param name="sourceValue"></param>
  /// <returns></returns>
  public static byte? GetByteValueNullable(this object sourceValue)
  {
    byte? value = null;

    if (sourceValue != null)
    {
      if (sourceValue is byte)
      {
        value = (byte)sourceValue;
      }
      else if (!string.IsNullOrWhiteSpace(sourceValue.ToString()))
      {
        value = Convert.ToByte(sourceValue);
      }
    }

    return value;
  }


  /// <summary>
  /// GetUnsignedIntegerValue Converts the sourceValue of DayOfWeek type to uint.
  /// </summary>
  /// <param name="sourceValue"></param>
  /// <returns></returns>
  public static uint GetUnsignedIntegerValue(this DateTime sourceValue)
  {
    return sourceValue.DayOfWeek.GetUnsignedIntegerValue();
  }

  /// <summary>
  /// GetUnsignedIntegerValue Converts the sourceValue of DayOfWeek type to uint.
  /// </summary>
  /// <param name="sourceValue"></param>
  /// <returns></returns>
  public static uint GetUnsignedIntegerValue(this DayOfWeek sourceValue)
  {
    var value = sourceValue != DayOfWeek.Sunday ? Convert.ToUInt32(sourceValue) : 7;

    return value;
  }
  public static double RemoveDecimals(this double input)
  {
    try { 
      return Math.Truncate(input);
    }
    catch
    {
      return 0;
    }
  }
}
