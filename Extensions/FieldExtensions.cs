using System;
using System.Collections.Generic;
using System.Linq;
using Ricoh.ricoh.deviceManagement;

// ReSharper disable once CheckNamespace
namespace Ricoh.Models
{
  static class FieldExtensions
  {
    /// <summary>
    /// Converts the raw device field into an external field object for consumption by the service.
    /// </summary>
    /// <param name="field">The raw device field to convert.</param>
    /// <returns>The converted ICounter value if valid; otherwise returns null.</returns>
    public static field ToField(this IDeviceField field)
    {
      if (field == null) return (null);
      
      return (new field() {
        name = field.Name,
        type = field.GetExternalType(),
        value = field.GetExternalValue()
      });
    }

    /// <summary>
    /// Converts the raw device field into an external field object for consumption by the service.
    /// </summary>
    /// <param name="fields">The raw devices fields to convert.</param>
    /// <returns>The converted ICounter value if valid; otherwise returns null.</returns>
    public static field[] ToFields(this IEnumerable<IDeviceField> fields)
    {
      if ((fields == null) || (!fields.Any())) return (null);
      
      return (fields.Select(c => c.ToField()).ToArray());
    }

    /// <summary>
    /// Checks to see if the field is one of the specified names.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <param name="names">The names of the fields to check.</param>
    /// <returns>True if the field is one of the specified value, false if otherwise.</returns>
    public static bool Is(this field field, params string[] names)
    {
      return (Is(field, (IEnumerable<string>) names));
    }

    /// <summary>
    /// Checks to see if the field is one of the specified names.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <param name="names">The names of the fields to check.</param>
    /// <returns>True if the field is one of the specified value, false if otherwise.</returns>
    public static bool Is(this field field, IEnumerable<string> names)
    {
      if (field == null) return(false);
      if (names == null) return(false);

      return (names.Any(n => String.Equals(n, field.name, StringComparison.CurrentCultureIgnoreCase)));
    }

    /// <summary>
    /// Checks to see if the field is one of the specified names.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <param name="accessControl">The access control type to check for.</param>
    /// <returns>True if the field is one of the specified value, false if otherwise.</returns>
    public static bool Is(this field field, DeviceAccessControl accessControl)
    {
      return (Is(field, accessControl.GetExternalNames()));
    }

    /// <summary>
    /// Checks to see if the field is related to fax control.
    /// </summary>
    /// <param name="field">The field to inspect.</param>
    /// <returns>True if the field is fax related, false if otherwise.</returns>
    public static bool IsFax(this field field)
    {
      return(Is(field, DeviceAccessControl.Fax));
    }

    /// <summary>
    /// Checks to see if the field is related to printer control.
    /// </summary>
    /// <param name="field">The field to inspect.</param>
    /// <returns>True if the field is printer related, false if otherwise.</returns>
    public static bool IsPrinter(this field field)
    {
      return (Is(field, DeviceAccessControl.Printer));
    }

    /// <summary>
    /// Checks to see if the field is related to copier control.
    /// </summary>
    /// <param name="field">The field to inspect.</param>
    /// <returns>True if the field is copier related, false if otherwise.</returns>
    public static bool IsCopier(this field field)
    {
      return (Is(field, DeviceAccessControl.Copier));
    }

    /// <summary>
    /// Checks to see if the field is related to scanner control.
    /// </summary>
    /// <param name="field">The field to inspect.</param>
    /// <returns>True if the field is scanner related, false if otherwise.</returns>
    public static bool IsScanner(this field field)
    {
      return (Is(field, DeviceAccessControl.Scanner));
    }

    /// <summary>
    /// Checks to see if the field is related to document server control.
    /// </summary>
    /// <param name="field">The field to inspect.</param>
    /// <returns>True if the field is document server related, false if otherwise.</returns>
    public static bool IsDocumentServer(this field field)
    {
      return (Is(field, DeviceAccessControl.DocumentServer));
    }
  }
}