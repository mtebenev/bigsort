using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.V2.Events;
using Microsoft.Extensions.Logging;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// Produces batches for bucket merging.
  /// Groups bucket chunks by infix and sends the batches to the chunk merge block.
  /// </summary>
  internal static class BucketMergeCoordinatorBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<ChunkFlushEvent, ChunkFlushEvent[]> Create(IPipelineContext pipelineContext)
    {
      var logger = pipelineContext.LoggerFactory.CreateLogger(nameof(BucketMergeCoordinatorBlock));
      var groupedEvents = new Dictionary<ushort, List<ChunkFlushEvent>>();
      var outgoingBlock = new BufferBlock<ChunkFlushEvent[]>();
      var incomingBlock = new ActionBlock<ChunkFlushEvent>(async evt =>
      {
        List<ChunkFlushEvent> bucketEvents;
        if(!groupedEvents.TryGetValue(evt.Infix, out bucketEvents))
        {
          bucketEvents = new List<ChunkFlushEvent>();
          groupedEvents.Add(evt.Infix, bucketEvents);
        }

        bucketEvents.Add(evt);
        logger.LogDebug("Added chunk, infix: {infix}", InfixUtils.InfixToString(evt.Infix));

        if(pipelineContext.IsBucketFullyFlushed(evt.Infix))
        {
          var chunks = groupedEvents[evt.Infix];
          await outgoingBlock.SendAsync(chunks.ToArray()).ConfigureAwait(false);
          chunks.Clear();
          logger.LogDebug("Pushed chunks to the bucket merge, infix: {infix}", InfixUtils.InfixToString(evt.Infix));
        }
      });

      incomingBlock.Completion.ContinueWith(t =>
      {
        // The chunks should be flushed when the incoming block gets all bucket's chunks.
        // The code below is an additional check that all the chunks already flushed.
        var remaining = groupedEvents
          .Where(kvp => kvp.Value.Count > 0)
          .Select(kvp => InfixUtils.InfixToString(kvp.Key))
          .ToList();

        if(remaining.Count > 0)
        {
          // This means that something goes wrong in the pipeline.
          var exception = new InvalidOperationException("Some of the chunks still not flushed.");
          ((ITargetBlock<ChunkFlushEvent[]>)outgoingBlock).Fault(exception);
          throw exception;
        }

        if(t.Status == TaskStatus.RanToCompletion)
        {
          // Noop
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
