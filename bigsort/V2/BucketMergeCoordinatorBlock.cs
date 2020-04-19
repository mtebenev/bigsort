using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BigSort.V2
{
  /// <summary>
  /// Produces batches for bucket merging.
  /// </summary>
  internal static class BucketMergeCoordinatorBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<BucketChunkFlushEvent, BucketChunkFlushEvent[]> Create()
    {
      var groupedEvents = new Dictionary<long, List<BucketChunkFlushEvent>>();
      var outgoingBlock = new BufferBlock<BucketChunkFlushEvent[]>(new DataflowBlockOptions { EnsureOrdered = true });
      var incomingBlock = new ActionBlock<BucketChunkFlushEvent>(async evt =>
      {
        List<BucketChunkFlushEvent> bucketEvents;
        if(!groupedEvents.TryGetValue(evt.Infix, out bucketEvents))
        {
          bucketEvents = new List<BucketChunkFlushEvent>();
          groupedEvents.Add(evt.Infix, bucketEvents);
        }

        bucketEvents.Add(evt);

        if(evt.IsFinalChunk)
        {
          Console.WriteLine($"SENDING FINAL CHUNK: {evt.Infix}");
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
          ((ITargetBlock<BucketChunkFlushEvent[]>)outgoingBlock).Fault(t.Exception.InnerException);
        }
        outgoingBlock.Complete();
      }, default, TaskContinuationOptions.ExecuteSynchronously,
      TaskScheduler.Default);

      return DataflowBlock.Encapsulate(incomingBlock, outgoingBlock);
    }
  }
}
