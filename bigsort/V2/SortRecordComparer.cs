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
        if(span1.Length == span2.Length)
        {
          for(int i = 0; i < span1.Length; i++)
          {
            var n1 = span1[i] - '0';
            var n2 = span2[i] - '0';
            if(n1 > n2)
            {
              result = 1;
              break;
            }
            else if(n1 < n2)
            {
              result = -1;
              break;
            }
          }
        }
        else
        {
          result = span1.Length > span2.Length ? 1 : -1;
        }
      }
      return result;
    }
  }
}
