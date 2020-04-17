using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;

namespace BigSort.V2
{
  /// <summary>
  /// Performs the final merge.
  /// Because our bucket merges are already sorted we just append them in the bucket order.
  /// </summary>
  internal static class FinalMerger
  {
    /// <summary>
    /// Performs merging.
    /// </summary>
    public static void Merge(IList<string> filePaths, string outFilePath)
    {
      var sources = filePaths.Select(p => File.ReadLines(p)).ToList();
      var target = sources.Aggregate((acc, l) => acc.Concat(l));
      
      File.WriteAllLines(outFilePath, target);
    }
  }
}
