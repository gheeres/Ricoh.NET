using System;

namespace Ricoh.Models
{
  /// <summary>
  /// Represents the summary of black and white counters.
  /// </summary>
  public class BlackWhiteColorCounter
  {
    private uint? _total;

    /// <summary>The total number of black copies.</summary>
    public uint Black { get; protected set; }

    /// <summary>The total number of color copies.</summary>
    public uint Color { get; protected set; }

    /// <summary>The total number of black/white and color copies.</summary>
    public uint Total 
    {
      get { return ((_total != null && (_total.Value > 0)) ? _total.Value : Black + Color); }
      protected set { _total = value; }
    }

    /// <param name="black">The total number of black copies.</param>
    /// <param name="color">The total number of color copies.</param>
    /// <param name="total">The total number of black/white and color copies.</param>
    public BlackWhiteColorCounter(uint black, uint color = 0, uint? total = null)
    {
      Black = black;
      Color = color;

      _total = (total != null) && (total.Value > 0) ? total : null;
    }

    /// <param name="black">The total number of black copies.</param>
    /// <param name="color">The total number of color copies.</param>
    /// <param name="total">The total number of black/white and color copies.</param>
    public BlackWhiteColorCounter(double black, double color = 0, double? total = null)
    {
      Black = Convert.ToUInt32(black);
      Color = Convert.ToUInt32(color);

      if ((total != null) && (total.Value > 0)) {
        _total = Convert.ToUInt32(total.Value);
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
      return (string.Format("B/W={0}, Color={1} ({2})", Black, Color, Total));
    }
  }
}