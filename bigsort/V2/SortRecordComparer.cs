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
      return string.Compare(x.Value, y.Value);
    }
  }
}
