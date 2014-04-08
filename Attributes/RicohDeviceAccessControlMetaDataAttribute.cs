using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  /// <summary>
  /// Allows for external metadata to be added to properities or classes which can be inspected at runtime.
  /// </summary>
  class RicohDeviceAccessControlMetaDataAttribute: Attribute
  {
    /// <summary>The external identifier for the access control.</summary>
    public IEnumerable<string> Names { get; set; }  
  
    /// <param name="names">The external identifiers for the access control.</param>
    public RicohDeviceAccessControlMetaDataAttribute(params string[] names)
    {
      Names = names;
    }
  }
}