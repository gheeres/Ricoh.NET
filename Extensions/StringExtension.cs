using System;

// ReSharper disable once CheckNamespace
namespace Ricoh
{
  static class StringExtension
  {
    /// <summary>
    /// Encodes the string to Base64
    /// </summary>
    /// <param name="input">The string to encode</param>
    /// <returns>The basae64 encoded string.</returns>
    public static string ToBase64(this string input)
    {
      if (input == null) return (null);
      return (Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input)));
    }

    /// <summary>
    /// Capitalize the first character in the string.
    /// </summary>
    /// <param name="input">The string to capitalize.</param>
    /// <returns></returns>
    public static string Capitalize(this string input)
    {
      if (input == null) return (null);

      char[] output = input.ToCharArray();
      int index = 0;
      while (index < output.Length) {
        if (char.IsLower(output[index])) {
          output[index] = char.ToUpper(output[index]);
          return(new String(output));
        }
        index++;
      }
      return (input);
    }
  }
}