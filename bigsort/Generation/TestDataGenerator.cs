using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using Microsoft.ConcurrencyVisualizer.Instrumentation;

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
        // Generate by chunks of 200mb
        var bufferSize = (long)StringUtils.ParseFileSize("200mb", 1024);
        bufferSize = Math.Min(toGenerateTotal, bufferSize);
        var bufferCount = toGenerateTotal / bufferSize;

        var chunkSizes = Enumerable.Repeat(bufferSize, (int)bufferCount);
        return chunkSizes;
      });

      // The generator block will produce data for a single chunk of a fixed size
      var generatorBlock = ChunkGeneratorBlock.Create(generatorFactory);

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
