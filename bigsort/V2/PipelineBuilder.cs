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
    public static (ITargetBlock<BufferReadEvent>, ITargetBlock<BucketMergeEvent[]>) Build(MergeSortOptions options, PipelineContext pipelineContext, int bufferSize)
    {
      // Buffer data in buckets
      var stringBufferBlock = StringBufferBlock.Create(pipelineContext, bufferSize);

      // Sort buckets
      var chunkSortBlock = ChunkSortBlock.Create(options);
      stringBufferBlock.LinkTo(chunkSortBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Flush buckets
      var chunkFlushBlock = ChunkFlushBlock.Create(pipelineContext);
      chunkSortBlock.LinkTo(chunkFlushBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Produce batches for bucket merge
      var bucketMergeCoordinatorBlock = BucketMergeCoordinatorBlock.Create(pipelineContext);
      chunkFlushBlock.LinkTo(bucketMergeCoordinatorBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Merge buckets
      var bucketMergeBlock = BucketMergeBlock.Create(options, pipelineContext);
      bucketMergeCoordinatorBlock.LinkTo(bucketMergeBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // The final batch
      var finalMergeBatchBlock = FinalMergeBatchBlock.Create(pipelineContext);
      bucketMergeBlock.LinkTo(finalMergeBatchBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // The final merge
      var finalMergeBlock = FinalMergeBlock.Create(pipelineContext);
      finalMergeBatchBlock.LinkTo(finalMergeBlock, new DataflowLinkOptions { PropagateCompletion = true });

      return (stringBufferBlock, finalMergeBlock);
    }
  }
}
