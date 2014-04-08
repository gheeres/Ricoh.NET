using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  public static class DeviceAccessControlExtensions
  {
    /// <summary>
    /// Indidates if the <see cref="DeviceAccessControl"/> has fax access control support.
    /// </summary>
    /// <param name="accessControl">The access control to inspect.</param>
    /// <returns>True if supported; false if otherwise.</returns>
    public static bool HasFaxAccessControl(this DeviceAccessControl accessControl)
    {
      return (accessControl.HasFlag(DeviceAccessControl.Fax));
    }

    /// <summary>
    /// Indidates if the <see cref="DeviceAccessControl"/> has copier access control support.
    /// </summary>
    /// <param name="accessControl">The access control to inspect.</param>
    /// <returns>True if supported; false if otherwise.</returns>
    public static bool HasCopierAccessControl(this DeviceAccessControl accessControl)
    {
      return (accessControl.HasFlag(DeviceAccessControl.Copier));
    }

    /// <summary>
    /// Indidates if the <see cref="DeviceAccessControl"/> has printer access control support.
    /// </summary>
    /// <param name="accessControl">The access control to inspect.</param>
    /// <returns>True if supported; false if otherwise.</returns>
    public static bool HasPrinterAccessControl(this DeviceAccessControl accessControl)
    {
      return (accessControl.HasFlag(DeviceAccessControl.Printer));
    }

    /// <summary>
    /// Indidates if the <see cref="DeviceAccessControl"/> has scanner access control support.
    /// </summary>
    /// <param name="accessControl">The access control to inspect.</param>
    /// <returns>True if supported; false if otherwise.</returns>
    public static bool HasScannerAccessControl(this DeviceAccessControl accessControl)
    {
      return (accessControl.HasFlag(DeviceAccessControl.Scanner));
    }

    /// <summary>
    /// Indidates if the <see cref="DeviceAccessControl"/> has document server access control support.
    /// </summary>
    /// <param name="accessControl">The access control to inspect.</param>
    /// <returns>True if supported; false if otherwise.</returns>
    public static bool HasDocumentServerAccessControl(this DeviceAccessControl accessControl)
    {
      return (accessControl.HasFlag(DeviceAccessControl.DocumentServer));
    }

    /// <summary>
    /// Indidates if the <see cref="DeviceAccessControl"/> has no access control support.
    /// </summary>
    /// <param name="accessControl">The access control to inspect.</param>
    /// <returns>True if supported; false if otherwise.</returns>
    public static bool HasNoAccessControl(this DeviceAccessControl accessControl)
    {
      return (accessControl == DeviceAccessControl.None);
    }

    /// <summary>
    /// Gets all of the external service names / identifiers from the specified access control.
    /// </summary>
    /// <param name="accessControl">The access control to inspect.</param>
    /// <returns>The collection of external names.</returns>
    internal static IEnumerable<string> GetExternalNames(this DeviceAccessControl accessControl)
    {
      Type type = typeof(DeviceAccessControl);

      var values = Enum.GetValues(type);
      foreach (int value in values) {
        if (((int)accessControl & value) == value) {
          string valueName = Enum.GetName(type, value);

          // Get the metadata
          FieldInfo field = accessControl.GetType().GetField(valueName);
          if (field != null) {
            var attributes = (RicohDeviceAccessControlMetaDataAttribute[])field.GetCustomAttributes(typeof(RicohDeviceAccessControlMetaDataAttribute), true);
            foreach (RicohDeviceAccessControlMetaDataAttribute attribute in attributes) {
              foreach(string name in attribute.Names) {
                yield return (name);
              }
            }
          }
        }
      }
    }
  }
}
