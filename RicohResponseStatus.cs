namespace Ricoh
{
  /// <summary>
  /// Class to contain the Ricoh response codes from the service.
  /// </summary>
  static class RicohResponseStatus
  {
    internal const string OK = "OK";
    internal const string TRUNCATED = "TRUNCATED";
    /// <summary>End / End of directory</summary>
    internal const string END_OF_DIRECTORY = "EOD";
    
    internal const string SOAP_ERROR = "COMMON_SOAP_SERVER";
    internal const string COMMON_BAD_PARAMETER = "COMMON_BAD_PARAMETER";

    internal const string UDIRECTORY_DIRECTORY_INCONSISTENT = "UDIRECTORY_DIRECTORY_INCONSISTENT";
  }
}
