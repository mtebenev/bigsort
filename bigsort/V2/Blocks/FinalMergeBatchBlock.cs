using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.V2.Events;
using MoreLinq;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// The block accumulates all merged buckets, orders them and sends the final merge batch.
  /// </summary>
  internal static class FinalMergeBatchBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<BucketMergeEvent, BucketMergeEvent[]> Create(IPipelineContext pipelineContext)
    {
      var bucketMergeEvents = new List<BucketMergeEvent>();
      var outgoingBlock = new BufferBlock<BucketMergeEvent[]>(new DataflowBlockOptions { EnsureOrdered = true });
      var incomingBlock = new ActionBlock<BucketMergeEvent>(evt =>
      {
        bucketMergeEvents.Add(evt);
      });

      incomingBlock.Completion.ContinueWith(async t =>
      {
        if(t.Status == TaskStatus.RanToCompletion)
        {
          await pipelineContext.GetAllInfixesAsync();
          var comparer = new InfixComparer();
          var orderedBuckets = bucketMergeEvents
          .OrderBy(e => e.Infix, comparer)
          .ToArray();
          await outgoingBlock.SendAsync(orderedBuckets).ConfigureAwait(false);
        }
        else if(t.IsFaulted)
        {
          ((ITargetBlock<BucketMergeEvent[]>)outgoingBlock).Fault(t.Exception.InnerException);
        }
        outgoingBlock.Complete();
      }, default, TaskContinuationOptions.ExecuteSynchronously,
      TaskScheduler.Default);

      return DataflowBlock.Encapsulate(incomingBlock, outgoingBlock);
    }
  }
}
