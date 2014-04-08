using System;
using Ricoh.Extensions;
using Ricoh.Models;
using Ricoh.ricoh.uDirectory;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  static class PropertyExtension
  {
    /// <summary>
    /// Checks to see if the property is valid or contains data
    /// </summary>
    /// <param name="property">The property to check</param>
    /// <returns>True if the property has a valid.</returns>
    public static bool HasValue(this property property)
    {
      if (property == null) return(false);

      return (! String.IsNullOrEmpty(property.propVal));
    }

    /// <summary>
    /// Creates and encapsulated property object from the specified value with associated name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The encapuslated property object.</returns>
    public static property ToProperty(this string value, string name)
    {
      if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "The property name cannot be empty or null.");

      return (new property() { propName = name, propVal = value });
    }

    /// <summary>
    /// Creates and encapsulated property object from the specified value with associated name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The encapuslated property object.</returns>
    public static property ToProperty(this int value, string name)
    {
      if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "The property name cannot be empty or null.");

      return (new property() { propName = name, propVal = Convert.ToString(value) });
    }

    /// <summary>
    /// Creates and encapsulated property object from the specified value with associated name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The encapuslated property object.</returns>
    public static property ToProperty(this uint value, string name)
    {
      if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "The property name cannot be empty or null.");

      return (new property() { propName = name, propVal = Convert.ToString(value) });
    }
    
    /// <summary>
    /// Creates and encapsulated property object from the specified value with associated name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The encapuslated property object.</returns>
    public static property ToProperty(this RicohEntryType value, string name)
    {
      if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "The property name cannot be empty or null.");

      return (new property() { propName = name, propVal = value.ToExternalServiceValue() });
    }

    /// <summary>
    /// Creates and encapsulated property object from the specified value with associated name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The encapuslated property object.</returns>
    public static property ToProperty(this bool value, string name)
    {
      if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "The property name cannot be empty or null.");

      return (new property() { propName = name, propVal = value.ToOnOff() });
    }
  }
}