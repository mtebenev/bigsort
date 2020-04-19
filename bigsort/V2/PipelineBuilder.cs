using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Blocks;
using BigSort.V2.Events;

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
    public static (ITargetBlock<BufferReadEvent>, ITargetBlock<BucketMergeEvent[]>) Build(MergeSortOptions options, IPipelineContext pipelineContext)
    {
      // Buffer data in buckets
      var bucketBufferBlock = StringBufferBlock.Create(pipelineContext);

      // Sort buckets
      var bucketSortBlock = ChunkSortBlock.Create(options);
      bucketBufferBlock.LinkTo(bucketSortBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Flush buckets
      var bucketFlushBlock = ChunkFlushBlock.Create(options, pipelineContext);
      bucketSortBlock.LinkTo(bucketFlushBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Produce batches for bucket merge
      var bucketMergeBatchBlock = BucketMergeCoordinatorBlock.Create();
      bucketFlushBlock.LinkTo(bucketMergeBatchBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Merge buckets
      var bucketMergeBlock = BucketMergeBlock.Create(options, pipelineContext);
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
