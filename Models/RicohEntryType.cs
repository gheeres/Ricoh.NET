using System;
using System.Collections.Generic;

namespace Ricoh.Models
{
  /// <summary>
  /// Represents the type of address book entry.
  /// </summary>
  public enum RicohEntryType
  {
    None = 0,
    User = 1,
    Group = 2
  }

  public static class RicohEntryTypeExtensions
  {
    /// <summary>
    /// GetValue's the internal session type used by the external service.
    /// </summary>
    /// <param name="type">The type to be converted.</param>
    /// <returns>The string representation required by the external service.</returns>
    internal static RicohEntryType FromExternalServiceValue(this string type)
    {
      IDictionary<string, RicohEntryType> map = new Dictionary<string, RicohEntryType>() {
        { "user", RicohEntryType.User },
        { "group", RicohEntryType.Group },
      };
      return (map[type]);
    }
    
    /// <summary>
    /// GetValue's the internal session type used by the external service.
    /// </summary>
    /// <param name="type">The type to be converted.</param>
    /// <returns>The string representation required by the external service.</returns>
    internal static string ToExternalServiceValue(this RicohEntryType type)
    {
      IDictionary<RicohEntryType, string> map = new Dictionary<RicohEntryType, string>() {
        { RicohEntryType.None, "user" },
        { RicohEntryType.User, "user" },
        { RicohEntryType.Group, "group" },
      };
      return (map[type]);
    }
  }
}