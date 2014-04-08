using System;
using Ricoh.ricoh.deviceManagement;

// ReSharper disable once CheckNamespace
namespace Ricoh.Models
{
  static class FieldCapabilityExtensions
  {
    /// <summary>
    /// Checks to see if the specified IRawDeviceCounter is a number.
    /// </summary>
    /// <param name="capability">The field to check.</param>
    /// <returns>True if the counter is a number; false if otherwise.</returns>
    public static bool IsNumber(this fieldCapability capability)
    {
      if (capability == null) return (false);
      return (String.Equals(capability.type,DeviceFieldFactory.DM_FIELD_UNSIGNED_INT, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Converts the specified <see cref="fieldCapability"/> into the default field representation.
    /// </summary>
    /// <param name="capability">The capability to convert to a default field representation.</param>
    /// <param name="value">The optional default value to assign the field.</param>
    /// <returns>The default field representation of the capability.</returns>
    public static field ToField(this fieldCapability capability, string value = null)
    {
      if (capability == null) return (null);
      return (new field() {
        name = capability.name,
        type = capability.type,
        value = value ?? capability.value
      });
    }
  }
}