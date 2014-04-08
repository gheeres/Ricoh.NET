using System;
using System.Collections;
using System.Linq;
using Ricoh.ricoh.uDirectory;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  /// <summary>
  /// An exception that occurs when the received Ricoh address book data is invalid.
  /// </summary>
  class InvalidRicohAddressBookEntryException : Exception
  {
    private readonly property[] _data;

    /// <summary>
    /// Gets a collection of key/value pairs that provide additional user-defined information about the exception.
    /// </summary>
    /// <returns>
    /// An object that implements the <see cref="T:System.Collections.IDictionary"/> interface and contains a collection of user-defined key/value pairs. The default is an empty collection.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public override IDictionary Data
    {
      get
      {
        if (_data == null) return(new Hashtable());
        return(_data.ToDictionary(x => x.propName, x => x.propVal));
      }
    }

    /// <param name="data">The raw data as returned by the service.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidRicohAddressBookEntryException(property[] data, Exception innerException = null):base("The specified address book entry is invalid", innerException)
    {
      _data = data;
    }
  }
}