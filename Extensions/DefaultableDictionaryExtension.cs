using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Ricoh.Models
{
  static class DefaultableDictionaryExtension
  {
    /// <summary>
    /// Wraps the dictionary with a defaultable dictionary that will return the default value if the specified key is not found.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the value stored in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to wrap with a default value.</param>
    /// <param name="defaultValue">The default value to return if the dictionary is not found.</param>
    /// <returns>The new defaultable dictionary.</returns>
    public static IDictionary<TKey, TValue> WithDefaultValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue defaultValue)
    {
      return (new DefaultableDictionary<TKey, TValue>(dictionary, defaultValue));
    }
  }
}