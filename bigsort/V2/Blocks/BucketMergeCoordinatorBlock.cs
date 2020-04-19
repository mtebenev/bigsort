using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.V2.Events;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// Produces batches for bucket merging.
  /// </summary>
  internal static class BucketMergeCoordinatorBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<ChunkFlushEvent, ChunkFlushEvent[]> Create()
    {
      var groupedEvents = new Dictionary<long, List<ChunkFlushEvent>>();
      var outgoingBlock = new BufferBlock<ChunkFlushEvent[]>(new DataflowBlockOptions { EnsureOrdered = true });
      var incomingBlock = new ActionBlock<ChunkFlushEvent>(async evt =>
      {
        List<ChunkFlushEvent> bucketEvents;
        if(!groupedEvents.TryGetValue(evt.Infix, out bucketEvents))
        {
          bucketEvents = new List<ChunkFlushEvent>();
          groupedEvents.Add(evt.Infix, bucketEvents);
        }

        bucketEvents.Add(evt);

        if(evt.IsFinalChunk)
        {
          var chunks = groupedEvents[evt.Infix];
          await outgoingBlock.SendAsync(chunks.ToArray()).ConfigureAwait(false);
          chunks.Clear();
        }
      }, new ExecutionDataflowBlockOptions { EnsureOrdered = true });

      incomingBlock.Completion.ContinueWith(async t =>
      {
        if(t.Status == TaskStatus.RanToCompletion)
        {
          // TODOA remove. All chunks should be out earler
          /*
          foreach(var l in groupedEvents.Values)
          {
            if(l.Count >= 0)
            {
              await outgoingBlock.SendAsync(l.ToArray()).ConfigureAwait(false);
              l.Clear();
            }
          }
          */
        }
        else if(t.IsFaulted)
        {
          ((ITargetBlock<ChunkFlushEvent[]>)outgoingBlock).Fault(t.Exception.InnerException);
        }
        outgoingBlock.Complete();
      }, default, TaskContinuationOptions.ExecuteSynchronously,
      TaskScheduler.Default);

      return DataflowBlock.Encapsulate(incomingBlock, outgoingBlock);
    }
  }
}
