using System;
using Ricoh.Models;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  /// <summary>
  /// An exception that occurs when an address book entry / authentication already exists.
  /// </summary>
  class DuplicateAddressBookEntryException : Exception
  {
    /// <param name="service">The service triggering the exception.</param>
    /// <param name="entry">The duplicate or invalid address book entry.</param>
    /// <param name="innerException">The inner exception.</param>
    public DuplicateAddressBookEntryException(RicohEmbeddedSoapService service, AddressBookEntry entry, Exception innerException = null):base(String.Format("The requested address book entry / authentication code {0} is already in use. ({1})", entry.Usercode, service.Hostname), innerException)
    {
    }
  }
}