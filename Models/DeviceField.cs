using System;
using System.Collections.Generic;
using Ricoh.Extensions;
using Ricoh.ricoh.deviceManagement;

namespace Ricoh.Models
{
  /// <summary>
  /// Class to encapsulate an external <see cref="field"/> object and associated <see cref="fieldCapability"/>.
  /// </summary>
  class DeviceField<T>: IDeviceField<T>
  {
    private readonly string _externalType;

    /// <summary>The name of device field</summary>
    public string Name { get; private set; }

    /// <summary>Get's the type of the value.</summary>
    public Type Type
    {
      get { return(typeof(T)); }
    }

    /// <summary>Indicates if the field is readable.</summary>
    public bool IsReadable { get; private set; }

    /// <summary>Indicates if the field is writable.</summary>
    public bool IsWritable { get; private set; }

    /// <summary>The actual value of the field.</summary>
    public T Value { get; private set; }

    /// <param name="name">The name of device field</param>
    /// <param name="value">The actual value of the field.</param>
    public DeviceField(string name, T value)
    {
      Name = name;
      Value = value;
    }

    /// <summary>The default value convertor.</summary>
    internal static Func<string, Type, T> DefaultValueConversion = (value, type) => (T)Convert.ChangeType(value, type);

    /// <param name="field">The external field value to parse.</param>
    /// <param name="capability">The capabilities of the field.</param>
    /// <param name="conversion">The function to execute to convert the value to the appropriate type.</param>
    internal DeviceField(field field, fieldCapability capability, Func<string, Type, T> conversion = null): this(field, new[] { capability }, conversion)
    {
    }

    /// <param name="field">The external field value to parse.</param>
    /// <param name="capabilities">All of the capabilities for the object to be searched for a match against.</param>
    /// <param name="conversion">The function to execute to convert the value to the appropriate type.</param>
    internal DeviceField(field field, IEnumerable<fieldCapability> capabilities, Func<string, Type, T> conversion = null)
    {
      if (field == null) throw new ArgumentNullException("field", "The external field was not provided from the external service call.");

      Name = field.name;
      _externalType = field.type;
      Value = conversion != null ? conversion(field.value, Type) : DefaultValueConversion(field.value, Type);

      // Assign the appropriate capability to the device.
      fieldCapability capability = capabilities.Get(field.name);
      if (capability != null) {
        IsReadable = capability.readable;
        IsWritable = capability.writable;
      }
    }

    /// <summary>
    /// The actual value of the field.
    /// </summary>
    /// <returns>The value for the field.</returns>
    public object GetValue()
    {
      return(Value);
    }

    /// <summary>
    /// The external type name for the field. Needed when sending the values back to the external service.
    /// </summary>
    /// <returns>The external name of the field if available; null if otherwise.</returns>
    public string GetExternalType()
    {
      return (_externalType);
    }

    /// <summary>
    /// Get's the external value for the field. This allows for the conversion of client side values to 
    /// the external representations of those values.
    /// </summary>
    /// <param name="externalType">The external type of value for the field. If not provided explicitly, then the value is retrieved internally.</param>
    /// <returns>The external string representation of the value.</returns>
    public string GetExternalValue(string externalType = null)
    {
      if (externalType == null) externalType = GetExternalType();

      IDictionary<string, Func<string>> map = new Dictionary<string, Func<string>>() {
        { DeviceFieldFactory.DM_FIELD_ENUM, () => Type == typeof(bool) ? ((bool) GetValue()).ToOnOff() : Convert.ToString(GetValue()) }  
      }.WithDefaultValue(() => Convert.ToString(GetValue()));
      
      return (map[externalType.ToUpper()].Invoke());
    }
  }
}