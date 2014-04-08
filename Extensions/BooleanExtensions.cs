using System;

namespace Ricoh.Extensions
{
  static class BooleanExtensions
  {
    /// <summary>
    /// Converts the boolean to the string representation of ON or OFF.
    /// </summary>
    /// <param name="value">The type to convert.</param>
    /// <returns>ON if the value is true, OFF if false.</returns>
    public static string ToOnOff(this bool value)
    {
      return (value ? "ON" : "OFF");
    }

    /// <summary>
    /// Converts the boolean to the string representation of ON or OFF.
    /// </summary>
    /// <param name="value">The type to convert.</param>
    /// <returns>ON if the value is true, OFF if false.</returns>
    public static bool FromOnOff(this string value)
    {
      return (String.Equals(value, "ON", StringComparison.OrdinalIgnoreCase) ||
              Convert.ToBoolean(value));
    }
  }
}
