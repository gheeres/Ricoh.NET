using System;
using Ricoh.ricoh.deviceManagement;

namespace Ricoh.Models
{
  /// <summary>
  /// Interface to encapsulate an external <see cref="field"/> object and associated <see cref="fieldCapability"/>.
  /// </summary>
  public interface IDeviceField
  {
    /// <summary>Get's the type of the value.</summary>
    Type Type { get; }

    /// <summary>The name of device field</summary>
    string Name { get; }

    /// <summary>Indicates if the field is readable.</summary>
    bool IsReadable { get; }

    /// <summary>Indicates if the field is writable.</summary>
    bool IsWritable { get; }

    /// <summary>
    /// The actual value of the field.
    /// </summary>
    /// <returns>The value for the field.</returns>
    object GetValue();

    /// <summary>
    /// The external type name for the field. Needed when sending the values back to the external service.
    /// </summary>
    /// <returns>The external name of the field if available; null if otherwise.</returns>
    string GetExternalType();

    /// <summary>
    /// Get's the external value for the field. This allows for the conversion of client side values to 
    /// the external representations of those values.
    /// </summary>
    /// <param name="externalType">The external type of value for the field. If not provided explicitly, then the value is retrieved internally.</param>
    /// <returns>The external string representation of the value.</returns>
    string GetExternalValue(string externalType = null);
  }

  /// <summary>
  /// Interface to encapsulate an external <see cref="field"/> object and associated <see cref="fieldCapability"/>.
  /// </summary>
  public interface IDeviceField<out T>: IDeviceField
  {
    /// <summary>The actual value of the field.</summary>
    T Value { get; }
  }
}