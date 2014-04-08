using System;
using System.Collections.Generic;
using System.Linq;
using Ricoh.ricoh.deviceManagement;

namespace Ricoh.Models
{
  /// <summary>
  /// Represents the base type of an object retrieved from the external device.
  /// </summary>
  public abstract class DeviceObject
  {
    private static object _mutex = new object();
    private IList<IDeviceField> _fields;

    /// <summary>The unique object id for the user on the device.</summary>
    public virtual uint Id { get; set; }
    /// <summary>The index of the entry.</summary>
    public abstract uint Index { get; }

    /// <summary>The unique authentication token for the user.</summary>
    public string Authentication { get; set; }
    /// <summary>The name of the user.</summary>
    public string Username { get; set; }

    /// <summary>The counters for the specified user.</summary>
    internal IEnumerable<IDeviceField> Fields
    {
      get { return (_fields ?? (_fields = new List<IDeviceField>())); }
      private set { _fields = new List<IDeviceField>(value); }
    }

    /// <summary>
    /// Extracts the index from the ObjectId. The Index is the last characters at the end of
    /// ObjectId with the baseValue stripped from the beginning. 
    /// </summary>
    /// <param name="objectId">The complete objectId with the embedded name / index.</param>
    /// <param name="baseValue">The base value for the objectId that should be removed.</param>
    /// <returns>The extracted name or index for the value.</returns>
    public static uint ExtractIndexFromObjectId(uint objectId, uint baseValue)
    {
      // The index is the last digits of the Id with the baseValue stripped from the beginning????
      // Using arithmetic instead of string conversion/substring selection
      uint digits = Convert.ToUInt32(Math.Floor(Math.Log10(objectId) + 1));
      uint powersOf10 = Convert.ToUInt32(Math.Pow(10, digits - 6));
      return (objectId - (baseValue * powersOf10));
    }

    /// <param name="id">The unique id for the user on the device.</param>
    protected DeviceObject(uint id)
    {
      Id = id;
    }

    /// <param name="id">The unique id for the user on the device.</param>
    /// <param name="fields">The raw data returned from the service</param>
    /// <param name="capabilities">The capabilities and limits of the fields.</param>
    protected DeviceObject(uint id, IEnumerable<field> fields, IEnumerable<fieldCapability> capabilities): this(id)
    {
      if (fields == null) throw new ArgumentNullException();

      // Map the fields to functions.
      IDictionary<string, Action<field>> map = new Dictionary<string, Action<field>>() {
        { "authname", (field) => Authentication = field.value },
        { "username", (field) => Username = field.value },
      }.WithDefaultValue((field) => OnUnknownField(field, capabilities.Get(field.name)));

      fields.AsParallel().ForAll(f => {
        if (f == null) return;

        var fieldName = f.name.ToLower();
        map[fieldName](f);
      });
    }

    /// <summary>
    /// Occurs when an unknown field is detected.
    /// </summary>
    /// <param name="field">The unknown field.</param>
    /// <param name="capability">The capabilities and limits describing the field.</param>
    protected virtual void OnUnknownField(field field, fieldCapability capability)
    {
      Add(DeviceFieldFactory.Create(field, capability));
    }

    /// <summary>
    /// Add the specified field item to the list of field. Null field items are ignored.
    /// </summary>
    /// <param name="field">The field to add. If null, then the field is ignored.</param>
    protected virtual void Add(IDeviceField field)
    {
      if (field == null) return;

      lock (_mutex) {
        if (_fields == null) _fields = new List<IDeviceField>();
        _fields.Add(field);
      }
    }
  }
}