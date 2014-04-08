using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  /// <summary>
  /// Allows for external metadata to be added to properities or classes which can be inspected at runtime.
  /// </summary>
  class RicohDeviceCapabilityMetaDataAttribute: Attribute
  {
    /// <summary>The external identifier for the access control.</summary>
    public string Name { get; set; }  
  
    /// <param name="name">The external identifiers for the access control.</param>
    public RicohDeviceCapabilityMetaDataAttribute(string name)
    {
      Name = name;
    }
  }
}