using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BigSort.V1
{
  /// <summary>
  /// Comparer for merge process.
  /// </summary>
  internal class MergeHeapNodeComparer : IComparer<MergeHeapNode>
  {
    public int Compare([AllowNull] MergeHeapNode x, [AllowNull] MergeHeapNode y)
    {
      return string.Compare(x.Data, y.Data);
    }
  }
}
