using System;
using System.Collections.Generic;

namespace Ricoh.Extensions
{
  public static class NumericExtensions
  {
    /// <summary>
    /// Hash of the numeric types.
    /// </summary>
    public static HashSet<Type> NumericTypes = new HashSet<Type> {
      typeof(byte), typeof(sbyte),
      typeof(short), typeof(ushort), 
      typeof(int), typeof(uint), 
      typeof(long), typeof(ulong),
      typeof(decimal)
    };

    /// <summary>
    /// Checks to see if the specified type is numeric.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the value is numeric, false if otherwise.</returns>
    public static bool IsNumeric(this Type type)
    {
      return (NumericTypes.Contains(type) ||
              NumericTypes.Contains(Nullable.GetUnderlyingType(type)));
    }
  }
}
