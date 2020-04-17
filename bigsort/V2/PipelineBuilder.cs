using System.Threading.Tasks.Dataflow;

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
    public static (ITargetBlock<StringBuffer>, ITargetBlock<SortBucket>) Build()
    {
      // Buffer data in buckets
      var bucketBufferBlock = BucketBufferBlock.Create();

      // Sort buckets
      var bucketSortBlock = BucketSortBlock.Create();
      bucketBufferBlock.LinkTo(bucketSortBlock);

      // Flush buckets
      var bucketFlushBlock = BucketFlushBlock.Create();
      bucketSortBlock.LinkTo(bucketFlushBlock);

      return (bucketBufferBlock, bucketFlushBlock);
    }
  }
}
