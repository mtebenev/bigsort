using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;
using Microsoft.ConcurrencyVisualizer.Instrumentation;

namespace BigSort.V2.Blocks
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
        for(var i = 0; i < events.Length; i++)
        {
          Console.WriteLine($"File {i}: {events[i].FilePath}");
        }

        var filePaths = events
          .Select(e => e.FilePath)
          .ToList();

        using(Markers.EnterSpan("Final merge"))
        {
          FinalMerger.Merge(filePaths, options.OutFilePath);
        }
      });

      return result;
    }
  }
}
