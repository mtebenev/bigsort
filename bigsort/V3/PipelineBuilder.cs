using System.Threading.Tasks.Dataflow;
using BigSort.Common;

namespace BigSort.V3
{
  /// <summary>
  /// Builds the whole pipeline.
  /// </summary>
  internal class PipelineBuilder
  {
    /// <summary>
    /// Builder method.
    /// </summary>
    public static (ITargetBlock<StringBuffer>, ITargetBlock<StringBuffer>) Build(MergeSortOptions options)
    {
      // Buffer data in buckets
      var partitionerBlock = PartitionerBlock.Create();

      return (partitionerBlock, partitionerBlock);
    }
  }
}
