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
      var span = s.AsSpan(this.DotPos + 2, 4); // Dot + space
      this.Infix = MemoryMarshal.Cast<char, long>(span)[0];
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
