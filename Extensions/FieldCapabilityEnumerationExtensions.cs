using System;
using System.Collections.Generic;
using System.Linq;
using Ricoh.ricoh.deviceManagement;

// ReSharper disable once CheckNamespace
namespace Ricoh.Models
{
  static class FieldCapabilityEnumerationExtensions
  {
    /// <summary>
    /// Retrieves the requested capabilities from the list. If not found, returnes the default value..
    /// </summary>
    /// <param name="capabilities">The fields to search.</param>
    /// <param name="name">The name of the capability to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the field is not found.</param>
    /// <returns>True if the counter is a number; false if otherwise.</returns>
    public static fieldCapability Get(this IEnumerable<fieldCapability> capabilities, string name, fieldCapability defaultValue = null)
    {
      if ((capabilities == null) || (! capabilities.Any())) return (defaultValue);

      fieldCapability capability = capabilities.FirstOrDefault(c => String.Equals(c.name, name, StringComparison.CurrentCultureIgnoreCase));
      return (capability ?? defaultValue);
    }

    /// <summary>
    /// Retrieves the fields from the field collection which can be sent to the
    /// external service for updates.
    /// </summary>
    /// <param name="capabilities">The collection of raw device counters.</param>
    /// <param name="filter">Additional filter criteria.</param>
    /// <returns>The external field array that can be passed for update.</returns>
    public static field[] GetUpdatableFields(this IEnumerable<fieldCapability> capabilities, Func<fieldCapability, bool> filter = null)
    {
      if (capabilities == null) return (new field[0]);
      return (capabilities.Where(c => c.writable && ((filter == null) || filter.Invoke(c)))
                          .Select(c => c.ToField())
                          .ToArray());
    }
  }
}