using System;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  /// <summary>
  /// An exception that occurs when an operation fails.
  /// </summary>
  class RicohOperationFailedException : Exception
  {
    /// <param name="service">The service triggering the exception.</param>
    /// <param name="operation">The name of the operation that failed.</param>
    /// <param name="innerException">The inner exception.</param>
    public RicohOperationFailedException(RicohEmbeddedSoapService service, string operation, Exception innerException = null):base(String.Format("The requested external operation on {0} ({1}) failed.", service.Hostname, operation), innerException)
    {
    }
  }
}