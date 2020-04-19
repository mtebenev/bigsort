using System.Threading.Tasks.Dataflow;
using BigSort.V2.Events;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// The block accumulates all merged buckets.
  /// </summary>
  internal static class FinalMergeBatchBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<BucketMergeEvent, BucketMergeEvent[]> Create()
    {
      var block = new BatchBlock<BucketMergeEvent>(
        9999999,
        new GroupingDataflowBlockOptions
        {
          BoundedCapacity = 9999999,
          MaxMessagesPerTask = 9999999,
          MaxNumberOfGroups = 1,
          Greedy = true
        });
      return block;
    }
  }
}
