using System;
using System.Collections.Generic;
using System.Linq;
using Ricoh.Extensions;
using Ricoh.ricoh.deviceManagement;

// ReSharper disable once CheckNamespace
namespace Ricoh.Models
{
  static class DeviceFieldEnumerationExtensions
  {
    /// <summary>
    /// Retrieve the specified device field by name.
    /// </summary>
    /// <param name="fields">The fields to search.</param>
    /// <param name="name">The name of the <see cref="IDeviceField" /> to find.</param>
    /// <returns>The <see cref="IDeviceField"/> matching the specified name.</returns>
    public static IDeviceField Get(this IEnumerable<IDeviceField> fields, string name)
    {
      return (Get(fields, new[] { name }).FirstOrDefault());
    }

    /// <summary>
    /// Retrieve the specified device field by the specified expression / filter.
    /// </summary>
    /// <param name="fields">The field to search.</param>
    /// <param name="filter">The custom filter to use to search.</param>
    /// <returns>The <see cref="IDeviceField"/> matching the specified name.</returns>
    public static IEnumerable<IDeviceField<T>> Get<T>(this IEnumerable<IDeviceField> fields, Func<IDeviceField, bool> filter)
    {
      return (Get(fields, filter).OfType<IDeviceField<T>>());
    }

    /// <summary>
    /// Retrieve the specified device field by the specified expression / filter.
    /// </summary>
    /// <param name="fields">The fields to search.</param>
    /// <param name="filter">The custom filter to use to search.</param>
    /// <returns>The <see cref="IDeviceField"/> matching the specified filter.</returns>
    public static IEnumerable<IDeviceField> Get(this IEnumerable<IDeviceField> fields, Func<IDeviceField, bool> filter)
    {
      if (fields == null) return(null);
      if (filter == null) throw new ArgumentNullException("filter", "No filter provided for search.");

      return (fields.Where(filter));
    }

    /// <summary>
    /// Retrieve the specified device field by name.
    /// </summary>
    /// <param name="fields">The field to search.</param>
    /// <param name="name">The name of the <see cref="IDeviceField" /> to find.</param>
    /// <returns>The <see cref="IDeviceField"/> matching the specified name.</returns>
    public static IDeviceField<T> Get<T>(this IEnumerable<IDeviceField> fields, string name)
    {
      return (Get<T>(fields, new[] { name }).FirstOrDefault());
    }

    /// <summary>
    /// Retrieve the specified device field by name.
    /// </summary>
    /// <param name="fields">The field to search.</param>
    /// <param name="names">The names of the <see cref="IDeviceField" /> to find.</param>
    /// <returns>The <see cref="IDeviceField"/> matching the specified name.</returns>
    public static IEnumerable<IDeviceField<T>> Get<T>(this IEnumerable<IDeviceField> fields, params string[] names)
    {
      return (Get(fields, names).OfType<IDeviceField<T>>());
    }
    
    /// <summary>
    /// Retrieve the specified device field by name.
    /// </summary>
    /// <param name="fields">The field to search.</param>
    /// <param name="names">The names of the <see cref="IDeviceField" /> to find.</param>
    /// <returns>The <see cref="IDeviceField"/> matching the specified name.</returns>
    public static IEnumerable<IDeviceField> Get(this IEnumerable<IDeviceField> fields, params string[] names)
    {
      if (fields == null) return(new IDeviceField[0]);
      if ((names == null) || (names.Length == 0)) return (new IDeviceField[0]);

      return (fields.Where(c => names.Any(n => String.Equals(c.Name, n, StringComparison.CurrentCultureIgnoreCase))));
    }

    /// <summary>
    /// Retrieve the specified field value.
    /// </summary>
    /// <param name="fields">The field to search.</param>
    /// <param name="name">The name of the <see cref="IDeviceField" /> to find.</param>
    /// <param name="defaultValue">The default value to use if the specified tag is not found.</param>
    /// <returns>The value of the specified counter. If not specified, then 0 is returned.</returns>
    public static T GetValue<T>(this IEnumerable<IDeviceField> fields, string name, T defaultValue = default(T))
    {
      IDeviceField<T> f = Get<T>(fields, name);
      return (f != null ? f.Value : defaultValue);
    }

    /// <summary>
    /// Retrieve the specified field value.
    /// </summary>
    /// <param name="fields">The field to search.</param>
    /// <param name="filter">The custom filter to use to search.</param>
    /// <param name="defaultValue">The default value to use if the specified tag is not found.</param>
    /// <returns>The value of the specified counter. If not specified, then 0 is returned.</returns>
    public static T GetValue<T>(this IEnumerable<IDeviceField> fields, Func<IDeviceField, bool> filter, T defaultValue = default(T))
    {
      IDeviceField<T> f = Get<T>(fields, filter).FirstOrDefault();
      return (f != null ? f.Value : defaultValue);
    }

    /// <summary>
    /// Check to see if any of the boolean <see cref="IDeviceField"/> values are true that match the specified name.
    /// </summary>
    /// <param name="fields">The fields to search.</param>
    /// <param name="names">The names of the <see cref="IDeviceField" /> to check.</param>
    /// <returns>True if any of the boolean values are true, false if otherwise, null if none of the specified names exist.</returns>
    public static bool? Any(this IEnumerable<IDeviceField> fields, IEnumerable<string> names)
    {
      if (fields == null) return (false);

      IDeviceField<bool> result = Get<bool>(fields, (f) => names.Any(n => String.Equals(f.Name, n, StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault();
      if (result == null) return (null);
      return (result.Value);
    }

    /// <summary>
    /// Retrieve the sum of the specified field values.
    /// </summary>
    /// <param name="fields">The fields to search.</param>
    /// <param name="names">The names of the <see cref="IDeviceField" /> to sum.</param>
    /// <returns>The value of the specified counter. If not specified, then 0 is returned.</returns>
    public static double Sum(this IEnumerable<IDeviceField> fields, params string[] names)
    {
      if (fields == null) return (0);
      
      return(Get(fields, names).Where(field => field.Type.IsNumeric())
                               .Sum(field => Convert.ToDouble(field.GetValue())));
    }

    /// <summary>
    /// Retrieves the fields from the <see cref="IDeviceField"/> which can be sent to the
    /// external service for updates.
    /// </summary>
    /// <param name="fields">The collection of raw device counters.</param>
    /// <param name="filter">Additional filter criteria.</param>
    /// <returns>The external field array that can be passed for update.</returns>
    public static IEnumerable<field> GetUpdatableFields(this IEnumerable<IDeviceField> fields, Func<IDeviceField, bool> filter = null)
    {
      if (fields == null) return (new field[0]);
      
      return (fields.Where(f => f.IsWritable && ((filter == null) || filter.Invoke(f)))
                    .Select(c => c.ToField())
                    .ToArray());
    }

    /// <summary>
    /// Outputs the specified fields to a string. Useful for debugging an viewing internal values.
    /// </summary>
    /// <param name="fields">The fields to enumerate to a string.</param>
    /// <returns>A command separated string of key value pairs.</returns>
    public static string ToString(this IEnumerable<IDeviceField> fields)
    {
      if (fields == null) return (null);

      return (String.Join(",", fields.Select(f => String.Format("{0}={1}", f.Name, f.GetValue()))));
    }
  }
}