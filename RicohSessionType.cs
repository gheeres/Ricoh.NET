namespace Ricoh
{
  /// <summary>
  /// Class to contain the Ricoh Session types.
  /// </summary>
  public enum RicohSessionType
  {
    /// <summary>Doesn't lock access to the device (S)</summary>
    SharedSession,
    /// <summary>Locks exclusive access to the device (X)</summary>
    ExclusiveSession
  }

  internal static class RicohSessionTypeExtensions
  {
    /// <summary>
    /// GetValue's the internal session type used by the external service.
    /// </summary>
    /// <param name="type">The type to be converted.</param>
    /// <returns>The string representation required by the external service.</returns>
    internal static string ToExternalServiceValue(this RicohSessionType type)
    {
      return (type == RicohSessionType.SharedSession ? "S" : "X");
    }
  }
}
