using System;
using System.Collections.Generic;
using System.Linq;
using Ricoh.ricoh.uDirectory;

namespace Ricoh.Models
{
  /// <summary>
  /// Represents an address book entry in the Ricoh uDirectory.
  /// </summary>
  public class AddressBookEntry
  {
    private List<property> _properties;
    private object _mutex = new object();

    /// <summary>The unique id for the entry.</summary>
    public uint Id
    {
      get { return(Convert.ToUInt32(_properties.GetValue("id"))); }
      protected set { Set(value.ToProperty("id")); }
    }
    /// <summary>The type of address book entry.</summary>
    public RicohEntryType EntryType
    {
      get { return(_properties.GetValue("entryType").FromExternalServiceValue()); }
      set { Set(value.ToProperty("entryType")); }
    }
    /// <summary>The name of the address book entry.</summary>
    public string Name
    {
      get { return(_properties.GetValue("name")); }
      set { Set(value.ToProperty("name")); }
    }
    /// <summary>The user code for the user.</summary>
    public string Usercode
    {
      get { return(_properties.GetValue("auth:name")); }
      set { Set(value.ToProperty("auth:name")); }
    }
    /// <summary>The display name for the user.</summary>
    public string DisplayName
    {
      get { return(_properties.GetValue("longName")); }
      set { Set(value.ToProperty("longName")); }
    }
    /// <summary>The display name for the user.</summary>
    public string EmailAddress
    {
      get { return(_properties.GetValue("mail:address")); }
      set { Set(value.ToProperty("mail:address")); }
    }

    /// <param name="entryType">The type of address book entry.</param>
    public AddressBookEntry(RicohEntryType entryType)
    {
      EntryType = entryType;
    }

    /// <param name="id">The unique id for the entry.</param>
    /// <param name="entryType">The type of address book entry.</param>
    public AddressBookEntry(uint id, RicohEntryType entryType = RicohEntryType.User)
    {
      Id = id;
      EntryType = entryType;
    }

    /// <param name="id">The unique id for the entry.</param>
    /// <param name="entryType">The type of address book entry.</param>
    /// <param name="name">The name of the address book entry.</param>
    public AddressBookEntry(uint id, RicohEntryType entryType, string name): this(id, entryType)
    {
      Name = name;
    }

    /// <param name="properties">The properties retrieved from the external service.</param>
    internal AddressBookEntry(property[] properties)
    {
      if (properties == null) throw new InvalidRicohAddressBookEntryException(null);

      _properties = new List<property>(properties);
      try {
        // Validate minimum parameters are available / valid
        var id = Convert.ToUInt32(_properties.GetValue("id"));
        var entryType = _properties.GetValue("entryType").FromExternalServiceValue();
        var name = _properties.GetValue("name");
        if ((id <= 0) || (entryType == RicohEntryType.None) || (String.IsNullOrEmpty(name))) {
          throw new InvalidRicohAddressBookEntryException(properties);
        }
      }
      catch (OverflowException exception) {
        throw new InvalidRicohAddressBookEntryException(properties, exception);
      }
      catch (FormatException exception) {
        throw new InvalidRicohAddressBookEntryException(properties, exception);
      }
    }

    /// <summary>
    /// Exports the data to the format needed by the external service.
    /// </summary>
    /// <param name="supportedProperties">The optional list of properties that the uDirectory service supports.</param>
    /// <returns>The array of properties to be serialized for the external service proxy.</returns>
    internal property[] ToProperties(string[] supportedProperties = null)
    {
      if (_properties == null) return (new property[0]);
      if (supportedProperties == null) return (_properties.ToArray());

      return (_properties.Where(p => supportedProperties.Contains(p.propName)).ToArray());
    }

    /// <summary>
    /// Exports the internal data structure to a dictionary.
    /// </summary>
    /// <returns>The array of properties from the external service proxy.</returns>
    public IDictionary<string, string> ToDictionary()
    {
      return (_properties.ToDictionary(p => p.propName, p => p.propVal));
    }

    /// <summary>
    /// Removes the internal service proxy value.
    /// </summary>
    /// <param name="name">The name of the property to modify.</param>
    public bool Clear(string name)
    {
      if (!String.IsNullOrEmpty(name)) {
        // Ensure this is thread safe.
        lock (_mutex) {
          var existing = _properties.SingleOrDefault(p => String.Equals(p.propName, name, StringComparison.OrdinalIgnoreCase));
          if (existing != null) {
            return(_properties.Remove(existing));
          }
        }
      }
      return (false);
    }

    /// <summary>
    /// Updates the associated property values.
    /// </summary>
    /// <param name="property"></param>
    internal void Set(property property)
    {
      if ((property == null) || (String.IsNullOrEmpty(property.propName))) return;
      if (_properties == null) _properties = new List<property>();

      // Ensure this is thread safe.
      lock(_mutex) {
        var existing = _properties.SingleOrDefault(p => String.Equals(p.propName, property.propName, StringComparison.OrdinalIgnoreCase));
        if (existing != null) existing.propVal = property.propVal;
        else _properties.Add(property);
      }
    }

    /// <summary>
    /// Allows for the internal service proxy values to be manipulated and changed.
    /// </summary>
    /// <param name="name">The name of the property to modify.</param>
    /// <param name="value">The value to change.</param>
    public void Set(string name, string value)
    {
      if (!String.IsNullOrEmpty(name)) {
        Set(new property() { propName = name, propVal = value });
      }
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
      return (String.Format("[{0:0000}] {1,-5} {2}", Id, EntryType, Name));
    }
  }
}
