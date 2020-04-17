using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BigSort.V2
{
  /// <summary>
  /// Produces batches for bucket merging.
  /// </summary>
  internal static class BucketMergeBatchBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<BucketFlushEvent, BucketFlushEvent[]> Create()
    {
      var groupedEvents = new Dictionary<long, List<BucketFlushEvent>>();
      var outgoingBlock = new BufferBlock<BucketFlushEvent[]>();
      var incomingBlock = new ActionBlock<BucketFlushEvent>(evt =>
      {
        List<BucketFlushEvent> bucketEvents;
        if(!groupedEvents.TryGetValue(evt.Infix, out bucketEvents))
        {
          bucketEvents = new List<BucketFlushEvent>();
          groupedEvents.Add(evt.Infix, bucketEvents);
        }

        bucketEvents.Add(evt);
      });

      incomingBlock.Completion.ContinueWith(async t =>
      {
        if(t.Status == TaskStatus.RanToCompletion)
        {
          foreach(var l in groupedEvents.Values)
          {
            if(l.Count >= 0)
            {
              await outgoingBlock.SendAsync(l.ToArray()).ConfigureAwait(false);
              l.Clear();
            }
          }
        }
        else if(t.IsFaulted)
        {
          ((ITargetBlock<BucketFlushEvent[]>)outgoingBlock).Fault(t.Exception.InnerException);
        }
        outgoingBlock.Complete();
      }, default, TaskContinuationOptions.ExecuteSynchronously,
      TaskScheduler.Default);

      return DataflowBlock.Encapsulate(incomingBlock, outgoingBlock);
    }
  }
}
