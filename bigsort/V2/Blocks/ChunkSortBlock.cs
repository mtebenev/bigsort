using System;
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
    public static TransformBlock<SortChunkBuffer, SortChunkBuffer> Create(MergeSortOptions options)
    {
      var block = new ChunkSortBlock();
      var result = new TransformBlock<SortChunkBuffer, SortChunkBuffer>(
        (bucket) => block.Execute(bucket),
        new ExecutionDataflowBlockOptions
        {
          BoundedCapacity = options.MaxConcurrentJobs,
          MaxDegreeOfParallelism = options.MaxConcurrentJobs,
        });

      return result;
    }

    private SortChunkBuffer Execute(SortChunkBuffer bucket)
    {
      using(Markers.EnterSpan("Bucket sort"))
      {
        var comparer = new SortRecordComparer();
        Array.Sort(bucket.SortRecordBuffer.Buffer, 0, bucket.SortRecordBuffer.BufferSize, comparer);
      }
      return bucket;
    }
  }
}
