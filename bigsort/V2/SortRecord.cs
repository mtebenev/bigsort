using System;
using System.Runtime.InteropServices;

namespace BigSort.V2
{
  /// <summary>
  /// Holds the string value along with supplimentary info for sorting.
  /// Design note: this is very specialized struct because we have strings in form:
  /// "(number). (string)"
  /// </summary>
  internal readonly struct SortRecord
  {
    /// <summary>
    /// Ctor.
    /// </summary>
    public SortRecord(string s)
    {
      this.DotPos = s.IndexOf('.');
      var span = s.AsSpan(this.DotPos + 2, Math.Min(4, s.Length - this.DotPos - 2)); // Dot + space
      if(span.Length >= 4)
      {
        this.Infix = MemoryMarshal.Cast<char, long>(span)[0];
      }
      else
      {
        this.Infix = 0;       
        this.Infix |= span[0];

        if(span.Length > 1)
        {
          this.Infix <<= 16;
          this.Infix |= span[1];
        }

        if(span.Length > 2)
        {
          this.Infix <<= 16;
          this.Infix |= span[2];
        }
      }
      this.Value = s;
    }

    /// <summary>
    /// The infix.
    /// </summary>
    public long Infix { get; }

    /// <summary>
    /// The position of the string part (after dot).
    /// </summary>
    public int DotPos { get; }

    /// <summary>
    /// The whole string value.
    /// </summary>
    public string Value { get; }
  }
}
