using System;
using System.Collections.Generic;
using System.Linq;
using Ricoh.ricoh.uDirectory;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  static class PropertyArrayExtension
  {
    /// <summary>
    /// Retrieves the specified property from the property array.
    /// </summary>
    /// <param name="properties">The properties to parse through</param>
    /// <param name="id">The id of the properties to retrieve.</param>
    /// <returns>The retrieve value if found, otherwise returns null.</returns>
    public static property Get(this IEnumerable<property> properties, string id)
    {
      if ((properties == null) || (! properties.Any())) return (null); 

      return(properties.SingleOrDefault(p => String.Equals(p.propName, id, StringComparison.CurrentCultureIgnoreCase)));
    }
    
    /// <summary>
    /// Retrieves the specified value from the property array.
    /// </summary>
    /// <param name="properties">The properties to parse through</param>
    /// <param name="id">The id of the properties to retrieve.</param>
    /// <returns>The retrieve value if found, otherwise returns null.</returns>
    public static string GetValue(this IEnumerable<property> properties, string id)
    {
      var property = properties.Get(id);
      return(property != null ? property.propVal : null);
    }
  }
}