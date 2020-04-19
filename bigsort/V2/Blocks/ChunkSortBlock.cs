using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using Microsoft.ConcurrencyVisualizer.Instrumentation;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// Responsible for sorting chunk content.
  /// </summary>
  internal class ChunkSortBlock
  {
    /// <summary>
    /// Ctor.
    /// </summary>
    private ChunkSortBlock()
    {
    }

    /// <summary>
    /// Factory.
    /// </summary>
    public static TransformBlock<SortBucket, SortBucket> Create(MergeSortOptions options)
    {
      var block = new ChunkSortBlock();
      var result = new TransformBlock<SortBucket, SortBucket>(
        (bucket) => block.Execute(bucket),
        new ExecutionDataflowBlockOptions
        {
          MaxDegreeOfParallelism = options.MaxConcurrentJobs,
          EnsureOrdered = true
        });

      return result;
    }

    private SortBucket Execute(SortBucket bucket)
    {
      using(Markers.EnterSpan("Bucket sort"))
      {
        var comparer = new SortRecordComparer();
        bucket.Records.Sort(comparer);
      }
      return bucket;
    }
  }
}
