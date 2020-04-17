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
    public static (ITargetBlock<StringBuffer>, ITargetBlock<BucketMergeEvent[]>) Build(MergeSortOptions options)
    {
      // Buffer data in buckets
      var bucketBufferBlock = BucketBufferBlock.Create();

      // Sort buckets
      var bucketSortBlock = BucketSortBlock.Create(options);
      bucketBufferBlock.LinkTo(bucketSortBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Flush buckets
      var bucketFlushBlock = BucketFlushBlock.Create(options);
      bucketSortBlock.LinkTo(bucketFlushBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Produce batches for bucket merge
      var bucketMergeBatchBlock = BucketMergeBatchBlock.Create();
      bucketFlushBlock.LinkTo(bucketMergeBatchBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Merge buckets
      var bucketMergeBlock = BucketMergeBlock.Create(options);
      bucketMergeBatchBlock.LinkTo(bucketMergeBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // The final batch
      var finalMergeBatchBlock = FinalMergeBatchBlock.Create();
      bucketMergeBlock.LinkTo(finalMergeBatchBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // The final merge
      var finalMergeBlock = FinalMergeBlock.Create(options);
      finalMergeBatchBlock.LinkTo(finalMergeBlock, new DataflowLinkOptions { PropagateCompletion = true });

      return (bucketBufferBlock, finalMergeBlock);
    }
  }
}
