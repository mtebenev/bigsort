using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using MoreLinq;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates necessary amount of the data.
  /// </summary>
  internal static class TestDataGenerator
  {
    /// <summary>
    /// Generates the data.
    /// </summary>
    public static ITargetBlock<StringBuffer2> Start(long toGenerateTotal, Func<ILineGenerator> generatorFactory, StreamWriter streamWriter)
    {
      // The batch block will split the whole volume by chunks
      var generatorBatchBlock = new TransformManyBlock<long, long>(size =>
      {
        // Generate by buffers of 50mb
        var bufferSize = (long)StringUtils.ParseFileSize("50mb", 1024);
        bufferSize = Math.Min(toGenerateTotal, bufferSize);
        var bufferCount = toGenerateTotal / bufferSize;

        var chunkSizes = Enumerable.Repeat(bufferSize, (int)bufferCount);
        return chunkSizes;
      });

      // The generator block will produce data for a single chunk of a fixed size
      var generatorBlock = new TransformManyBlock<long, StringBuffer2>(chunkSize =>
      {
        var lineGenerator = generatorFactory();
        var buffers = TestDataGenerator.GenerateBuffers(lineGenerator, chunkSize);
        return buffers;
      }, 
      new ExecutionDataflowBlockOptions 
      {
        MaxDegreeOfParallelism = 6,
        MaxMessagesPerTask = int.MaxValue,
        BoundedCapacity = int.MaxValue
      });

      // The writer block will flush the data
      var writerBlock = new ActionBlock<StringBuffer2>(buffer =>
      {
        TestDataGenerator.SaveBuffer(buffer, streamWriter);
      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

      // Configure the pipeline
      generatorBatchBlock.LinkTo(generatorBlock, new DataflowLinkOptions { PropagateCompletion = true });
      generatorBlock.LinkTo(writerBlock, new DataflowLinkOptions { PropagateCompletion = true });

      // Start the pipeline
      generatorBatchBlock.Post(toGenerateTotal);
      generatorBatchBlock.Complete();

      return writerBlock;
    }

    /// <summary>
    /// Produces multiple string buffers to produce a chunk of required size.
    /// Note: the result size could be slightly bigger than required, but it's not important for this task.
    /// </summary>
    private static IEnumerable<StringBuffer2> GenerateBuffers(ILineGenerator lineGenerator, long chunkSize)
    {
      var maxBufferSize = chunkSize / 10;
      var totalGenerated = 0; // Count total symbols in this chunk
      while(totalGenerated < chunkSize)
      {
        var memBuffer = MemoryPool<char>.Shared.Rent((int)maxBufferSize);

        var generatedSymbols = lineGenerator.FillBuffer(memBuffer.Memory.Span, chunkSize - totalGenerated);
        totalGenerated += generatedSymbols;

        var stringBuffer = new StringBuffer2(memBuffer, generatedSymbols);
        yield return stringBuffer;
      }
    }

    private static void SaveBuffer(StringBuffer2 buffer, StreamWriter streamWriter)
    {
      using(Markers.EnterSpan("Saving data buffer"))
      {
        var span = buffer.Buffer.Memory.Span.Slice(0, buffer.SymbolCount);
        StringBufferWriter.WriteBuffer(span, streamWriter);
      }

      buffer.Buffer.Dispose();
    }
  }
}
