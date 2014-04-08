using System;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  /// <summary>
  /// An exception that occurs when an operation fails.
  /// </summary>
  class RicohOperationTimedOutException : Exception
  {
    /// <param name="service">The service triggering the exception.</param>
    /// <param name="operation">The name of the operation that failed.</param>
    /// <param name="innerException">The inner exception.</param>
    public RicohOperationTimedOutException(RicohEmbeddedSoapService service, string operation, Exception innerException = null):base(String.Format("The requested external operation on {0} ({1}) timed out.", service.Hostname, operation), innerException)
    {
    }
  }
}