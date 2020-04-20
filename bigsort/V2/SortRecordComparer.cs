using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BigSort.V2
{
  /// <summary>
  /// Compares sort records.
  /// </summary>
  internal class SortRecordComparer : IComparer<SortRecord>
  {
    public int Compare([AllowNull] SortRecord x, [AllowNull] SortRecord y)
    {
      var result = string.CompareOrdinal(x.Value, x.DotPos + 1, y.Value, y.DotPos + 1, int.MaxValue);
      if(result == 0)
      {
        var span1 = x.Value.AsSpan(0, x.DotPos);
        var span2 = y.Value.AsSpan(0, y.DotPos);
        result = span1.CompareTo(span2, StringComparison.Ordinal);
      }
      return result;
    }
  }
}
