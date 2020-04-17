using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;

namespace BigSort.V2
{
  /// <summary>
  /// Performs the final merge.
  /// </summary>
  internal class FinalMergeBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static ITargetBlock<BucketMergeEvent[]> Create(MergeSortOptions options)
    {
      var result = new ActionBlock<BucketMergeEvent[]>(events =>
      {
        Console.WriteLine($"FinalMergeBlock.Execute(). Files: ");
        for(int i = 0; i < events.Length; i++)
        {
          Console.WriteLine($"File {i}: {events[i].FilePath}");
        }

        var filePaths = events
          .Select(e => e.FilePath)
          .ToList();

        FinalMerger.Merge(filePaths, options.OutFilePath);
      });

      return result;
    }
  }
}
