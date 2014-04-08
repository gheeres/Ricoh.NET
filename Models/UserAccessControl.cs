using System;
using System.Collections.Generic;
using System.Linq;
using Ricoh.ricoh.deviceManagement;

namespace Ricoh.Models
{
  /// <summary>
  /// Represents the restrictions for the device.
  /// </summary>
  public class UserAccessControl: DeviceObject
  {
    internal const uint USERRESTRICTION_OBJECTID = 110002;

    private static object _mutex = new object();

    /// <summary>The index of the entry.</summary>
    public override uint Index
    {
      get { return (ExtractIndexFromObjectId(Id)); }
    }

    /// <summary>Indicates if fax support is enabled.</summary>
    public bool Fax
    {
      get { return (IsAllowed(DeviceAccessControl.Fax.GetExternalNames())); } // "faxSend"
    }

    /// <summary>Indicates if copier support is enabled or allowed.</summary>
    public bool Copier
    {
      get { return (IsAllowed(DeviceAccessControl.Copier.GetExternalNames())); } // "copyBlack", "copyFull", "copyMono"
    }

    /// <summary>Indicates if scanner support is enabled or allowed.</summary>
    public bool Scanner
    {
      get { return (IsAllowed(DeviceAccessControl.Scanner.GetExternalNames())); } // "scannerBlack", "scannerFull"
    }

    /// <summary>Indicates if printer support is enabled or allowed.</summary>
    public bool Printer
    {
      get { return (IsAllowed(DeviceAccessControl.Printer.GetExternalNames())); } // "printerBlack", "printerFull"
    }

    /// <summary>Indicates if document server storage is allowed.</summary>
    public bool DocumentServer
    {
      get { return (IsAllowed(DeviceAccessControl.DocumentServer.GetExternalNames())); } // "localStorage"
    }

    /// <param name="id">The unique id for the user on the device.</param>
    public UserAccessControl(uint id): base(id)
    {
    }

    /// <param name="id">The unique id for the user on the device.</param>
    /// <param name="fields">The raw data returned from the service</param>
    /// <param name="capabilities">The capabilities and limits of the fields.</param>
    internal UserAccessControl(uint id, IEnumerable<field> fields, IEnumerable<fieldCapability> capabilities): base(id, fields, capabilities)
    {
    }

    /// <summary>
    /// Extracts the index from the ObjectId. The Index is the last characters at the end of
    /// ObjectId with the baseValue stripped from the beginning. 
    /// </summary>
    /// <param name="objectId">The complete objectId with the embedded name / index.</param>
    /// <returns>The extracted name or index for the value.</returns>
    public static uint ExtractIndexFromObjectId(uint objectId)
    {
      return (ExtractIndexFromObjectId(objectId, USERRESTRICTION_OBJECTID));
    }

    /// <summary>
    /// Checks to see if access to the specified function is allowed.
    /// </summary>
    /// <param name="names">The external field names to check for permission.</param>
    /// <returns>True if access is allowed; false if otherwise</returns>
    private bool IsAllowed(params string[] names)
    {
      return (IsAllowed((IEnumerable<string>) names));
    }

    /// <summary>
    /// Checks to see if access to the specified function is allowed.
    /// </summary>
    /// <param name="names">The external field names to check for permission.</param>
    /// <returns>True if access is allowed; false if otherwise</returns>
    private bool IsAllowed(IEnumerable<string> names)
    {
      // Externally the service uses ON/OFF to indicate if access is RESTRICTED.
      // Our interface uses true to indicate if access is ENABLED, false if RESTRICTED.
      bool? any = Fields.Any(names);
      if (any == null) return(false);
      return (! any.Value);
    }
    
    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    /// A string that represents the current object.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public override string ToString()
    {
      return (String.Format("[{0:0000}] {1,-8} - {2,-32}: {3}{4}{5}{6}{7}", Index, Authentication, Username, 
                            Copier ? 'C' : ' ', 
                            Printer ? 'P' : ' ', 
                            Scanner ? 'S' : ' ',
                            Fax ? 'F' : ' ',
                            DocumentServer ? 'D' : ' '));
    }
  }
}
