using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ricoh.ricoh.deviceManagement;

// ReSharper disable once CheckNamespace
namespace Ricoh.Models
{
  static class FieldArrayExtensions
  {
    /// <summary>
    /// Set's the values for all of the fields to the specified value.
    /// </summary>
    /// <param name="fields">The fields to set the value.</param>
    /// <param name="value">The value to set for all of the fields.</param>
    public static IEnumerable<field> Clear(this IEnumerable<field> fields, string value = "0")
    {
      if (fields == null) return(new field[0]);

      fields.AsParallel().ForAll(f => {
        f.value = value;
      });
      return (fields);
    }

    /// <summary>
    /// Outputs the specified fields to a string. Useful for debugging an viewing internal values.
    /// </summary>
    /// <param name="fields">The fields to enumerate to a string.</param>
    /// <returns>A command separated string of key value pairs.</returns>
    public static string ToString(this IEnumerable<field> fields)
    {
      if (fields == null) return (null);

      return (String.Join(",", fields.Select(f => String.Format("{0}=[{1}]{2}", f.name, f.type, f.value))));
    }
  }
}