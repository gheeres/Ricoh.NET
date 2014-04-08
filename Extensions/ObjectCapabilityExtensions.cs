using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ricoh.ricoh.deviceManagement;

// ReSharper disable once CheckNamespace
namespace Ricoh.Models
{
  static class ObjectCapabilityExtensions
  {
    /// <summary>
    /// Gets all of the external service names / identifiers from the specified access control.
    /// </summary>
    /// <param name="capability">The object capability to extract the name from</param>
    /// <returns>The collection of external names.</returns>
    internal static DeviceAccessControl ToDeviceAccessControl(this objectCapability capability)
    {
      if (capability == null) return (DeviceAccessControl.None);

      Type type = typeof(DeviceAccessControl);
      var names = Enum.GetNames(type);
      foreach (string name in names) {
        // Get the metadata
        FieldInfo field = type.GetField(name);
        if (field != null) {
          var attributes = (RicohDeviceCapabilityMetaDataAttribute[])field.GetCustomAttributes(typeof(RicohDeviceCapabilityMetaDataAttribute), true);
          if (attributes.Any(attribute => String.Equals(attribute.Name, capability.name, StringComparison.CurrentCultureIgnoreCase))) {
            return ((DeviceAccessControl) Enum.Parse(type, name));
          }
        }
      }

      Trace.TraceWarning("Unknown {0} received: {1}", DeviceManagement.USAGE_CONTROL_DEVICE, capability.name);
      return (DeviceAccessControl.None);
    }
  }
}