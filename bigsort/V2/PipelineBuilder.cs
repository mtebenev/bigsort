using System.Threading.Tasks.Dataflow;
using BigSort.Common;

namespace BigSort.V2
{
  /// <summary>
  /// Builds the whole pipeline.
  /// </summary>
  internal class PipelineBuilder
  {
    /// <summary>
    /// Builder method.
    /// </summary>
    public static (ITargetBlock<StringBuffer>, ITargetBlock<BucketFlushEvent[]>) Build(MergeSortOptions options)
    {
      // Buffer data in buckets
      var bucketBufferBlock = BucketBufferBlock.Create();

      // Sort buckets
      var bucketSortBlock = BucketSortBlock.Create();
      bucketBufferBlock.LinkTo(bucketSortBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Flush buckets
      var bucketFlushBlock = BucketFlushBlock.Create(options);
      bucketSortBlock.LinkTo(bucketFlushBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Produce batches for bucket merge
      var bucketMergeBatchBlock = BucketMergeBatchBlock.Create();
      bucketFlushBlock.LinkTo(bucketMergeBatchBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Merge buckets
      var bucketMergeBlock = BucketMergeBlock.Create();
      bucketMergeBatchBlock.LinkTo(bucketMergeBlock, new DataflowLinkOptions { PropagateCompletion = true });

      return (bucketBufferBlock, bucketMergeBlock);
    }
  }
}
