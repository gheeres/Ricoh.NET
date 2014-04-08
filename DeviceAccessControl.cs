using System;

namespace Ricoh
{
  /// <summary>
  /// Defines the known / published access control lists.
  /// </summary>
  [Flags]
  public enum DeviceAccessControl
  {
    None = 0,

    [RicohDeviceCapabilityMetaData("faxSend")]
    [RicohDeviceAccessControlMetaData("faxSend")]
    Fax = 1,
    [RicohDeviceCapabilityMetaData("copy")]
    [RicohDeviceAccessControlMetaData("copyMono", "copyBlack", "copyTwin", "copyFull")]
    Copier = 2,
    [RicohDeviceCapabilityMetaData("printer")]
    [RicohDeviceAccessControlMetaData("printerBlack", "printerFull")]
    Printer = 4,
    [RicohDeviceCapabilityMetaData("scanner")]
    [RicohDeviceAccessControlMetaData("scannerBlack", "scannerFull")]
    Scanner = 8,
    [RicohDeviceCapabilityMetaData("localStorage")]
    [RicohDeviceAccessControlMetaData("localStorage")]
    DocumentServer = 16,

    All = 2147483647
  }
}