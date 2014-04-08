using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ricoh.ricoh.deviceManagement;

namespace Ricoh.Models
{
  
  /// <summary>
  /// Factory for creating <see cref="IDeviceField"/> entries
  /// </summary>
  class DeviceFieldFactory
  {
    internal const string DM_FIELD_STRING = "DM_FIELD_STRING";
    internal const string DM_FIELD_UNSIGNED_INT = "DM_FIELD_UNSIGNED_INT";
    internal const string DM_FIELD_ENUM = "DM_FIELD_ENUM";

    /// <summary>
    /// Constructor to create an <see cref="IDeviceField"/> from the external <see cref="field"/> object.
    /// </summary>
    /// <param name="field">The external <see cref="field"/> to encapsulate.</param>
    /// <param name="capability">The capabilties describing the field.</param>
    /// <returns>An <see cref="IDeviceField"/> object.</returns>
    public static IDeviceField Create(field field, fieldCapability capability = null)
    {
      if (field == null) throw new ArgumentNullException("field", "The external field was not provided from the external service call.");

      IDictionary<string, Func<field, fieldCapability, IDeviceField>> typeMap = new Dictionary<string, Func<field, fieldCapability, IDeviceField>>() {
        { DM_FIELD_STRING, (f, c) => new DeviceField<string>(f, c) },
        { DM_FIELD_UNSIGNED_INT, (f, c) => new DeviceField<uint>(f, c) },
        { DM_FIELD_ENUM, (f, c) => {
          //TODO: Parse the known enumeration types
          // "ON, OFF"
          return (new DeviceField<bool>(f, c, (name, type) => {
            if (String.Equals("ON", f.value, StringComparison.CurrentCultureIgnoreCase)) return (true);
            if (String.Equals("OFF", f.value, StringComparison.CurrentCultureIgnoreCase)) return (false);
            
            Trace.TraceWarning("Unknown {0} detected. Values: [{1}] {2}", DM_FIELD_ENUM, c.rangeType, String.Join(",", c.valueEnum));
            return (DeviceField<bool>.DefaultValueConversion(name, type));
          }));
        }}
      }.WithDefaultValue((f, c) => {
        Trace.TraceWarning("Unknown fieldType {0} detected. Name: {1}, Value: {2}", field.type, field.name, field.value);
        return (null);
      });

      return (typeMap[field.type.ToUpper()].Invoke(field, capability));
    }

    /// <summary>
    /// Constructor to create an <see cref="IDeviceField"/> from the external <see cref="field"/> object.
    /// </summary>
    /// <param name="field">The external <see cref="field"/> to encapsulate.</param>
    /// <param name="capabilities">The capabilties describing possible <see cref="field"/> values.</param>
    /// <returns>An <see cref="IDeviceField"/> object.</returns>
    public static IDeviceField Create(field field, IEnumerable<fieldCapability> capabilities = null)
    {
      return (Create(field, capabilities.Get(field.name)));
    }
  }
}