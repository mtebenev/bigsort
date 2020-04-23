using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;

namespace BigSort.Generation
{
  /// <summary>
  /// Given a chunk size, generates data by smaller blocks.
  /// </summary>
  internal class ChunkGeneratorBlock
  {
    public static IPropagatorBlock<long, StringBuffer2> Create(Func<ILineGenerator> generatorFactory)
    {
      // Note: although we could accept the outgoing block as argument, we create the outgoing block here to 
      // keep building pipeline in a single place.
      var outgoingBlock = new BufferBlock<StringBuffer2>(
        new DataflowBlockOptions 
        {
          BoundedCapacity = 1
        }
      );
      var incomingBlock = new ActionBlock<long>(
        chunkSize => ChunkGeneratorBlock.GenerateBuffersAsync(generatorFactory, chunkSize, outgoingBlock),
        new ExecutionDataflowBlockOptions
        {
          BoundedCapacity = 1,
          MaxDegreeOfParallelism = Environment.ProcessorCount
        }
      );

      incomingBlock.Completion.ContinueWith(t =>
      {
        if(t.Status == TaskStatus.RanToCompletion)
        {
          // Noop
        }
        else if(t.IsFaulted)
        {
          ((ITargetBlock<StringBuffer2>)outgoingBlock).Fault(t.Exception.InnerException);
        }
        outgoingBlock.Complete();
      }, default, TaskContinuationOptions.ExecuteSynchronously,
      TaskScheduler.Default);

      return DataflowBlock.Encapsulate(incomingBlock, outgoingBlock);
    }

    /// <summary>
    /// Produces multiple string buffers to produce a chunk of required size.
    /// Note: the result size could be slightly bigger than required, but it's not important for this task.
    /// </summary>
    private static async Task GenerateBuffersAsync(Func<ILineGenerator> generatorFactory, long chunkSize, ITargetBlock<StringBuffer2> target)
    {
      var lineGenerator = generatorFactory();
      var maxBufferSize = chunkSize / 10;
      var totalGenerated = 0; // Count total symbols in this chunk
      while(totalGenerated < chunkSize)
      {
        var memBuffer = MemoryPool<char>.Shared.Rent((int)maxBufferSize);

        var generatedSymbols = lineGenerator.FillBuffer(memBuffer.Memory.Span, chunkSize - totalGenerated);
        totalGenerated += generatedSymbols;

        var stringBuffer = new StringBuffer2(memBuffer, generatedSymbols);
        var isEnqueued = await target.SendAsync(stringBuffer);
        if(!isEnqueued)
        {
          throw new InvalidOperationException("Unable to enqueue data buffer for flushing.");
        }
      }
    }

  }
}
