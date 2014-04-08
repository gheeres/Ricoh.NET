using System;
using System.Collections.Generic;
using System.Linq;
using Ricoh.ricoh.deviceManagement;

namespace Ricoh.Models
{
  /// <summary>
  /// Represents the counter totals for the printer / copier for the specified user.
  /// </summary>
  public class UserCounter: DeviceObject
  {
    internal const uint USERCOUNTER_OBJECTID = 111002;

    private static object _mutex = new object();

    /// <summary>The index of the entry.</summary>
    public override uint Index 
    {
      get { return (ExtractIndexFromObjectId(Id)); }
    }

    /// <summary>The total number of scans that were done.</summary>
    public BlackWhiteColorCounter Scanner
    {
      get
      {
        return (new BlackWhiteColorCounter(Fields.Sum("scannerBlack", "scannerBlackA3Over"),
                                           Fields.Sum("scannerFull", "scannerFullA3Over"),
                                           Fields.GetValue<uint>("scanTotal")));
      }
    }

    /// <summary>The total number of pages printed</summary>
    public BlackWhiteColorCounter Total
    {
      get 
      {
        return (new BlackWhiteColorCounter(Fields.GetValue<uint>("blackAccount" /*blackTotal*/),
                                           Fields.GetValue<uint>("colorAccount" /*colorTotal*/)));
      }
    }

    /// <summary>The total number of pages printed via the copier.</summary>
    public BlackWhiteColorCounter Copier
    {
      get
      {
        return (new BlackWhiteColorCounter(Fields.Sum("copyMono", "copyMonoA3Over", "copyBlack", "copyBlackA3Over"),
                                           Fields.Sum("copyTwin", "copyTwinA3Over", "copyFull", "copyFullA3Over")));
      }
    }

    /// <summary>The total number of pages printed via the IPP printer.</summary>
    public BlackWhiteColorCounter Printer
    {
      get
      {
        return (new BlackWhiteColorCounter(Fields.Sum("printerMono", "printerMonoA3Over", "printerBlack", "printerBlackA3Over"),
                                           Fields.Sum("printerTwin", "printerTwinA3Over", "printerFull", "printerFullA3Over")));
      }
    }

    /// <summary>The total number of fax pages printed.</summary>
    public uint Fax
    {
      get { return (Convert.ToUInt32(Fields.Sum("faxMono", "faxMonoA3Over", "faxBlack", "faxBlackA3Over"))); }
    }
    /// <summary>The total number of fax pages sent.</summary>
    public uint Sent
    {
      get { return (Fields.GetValue<uint>("faxSend")); }
    }

    /// <param name="id">The unique id for the user on the device.</param>
    /// <param name="authentication">The unique authentication token for the user.</param>
    /// <param name="username">The name of the user.</param>
    public UserCounter(uint id, string authentication, string username): base(id)
    {
      Authentication = authentication;
      Username = username;
    }

    /// <param name="id">The unique id for the user on the device.</param>
    /// <param name="fields">The raw data returned from the service</param>
    /// <param name="capabilities">The capabilities and limits of the fields.</param>
    internal UserCounter(uint id, IEnumerable<field> fields, IEnumerable<fieldCapability> capabilities): base(id, fields, capabilities)
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
      return (ExtractIndexFromObjectId(objectId, USERCOUNTER_OBJECTID));
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
      return (String.Format("[{0:0000}] {1,-8} - {2,-32}: {3}", Index, Authentication, Username, Total));
    }
  }
}
