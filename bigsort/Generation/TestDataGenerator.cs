using System;
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
    public static ITargetBlock<StringBuffer> Start(long toGenerateTotal, Func<ILineGenerator> generatorFactory, StreamWriter streamWriter)
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

      // The generator block will produce data for a single chunk
      var generatorBlock = new TransformBlock<long, StringBuffer>(bufferSize =>
      {
        var lineGenerator = generatorFactory();
        var (buffer, generatedBlockSize) = TestDataGenerator.GenerateBuffer(lineGenerator, bufferSize);
        return buffer;
      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 8 });

      // The writer block will flush the data
      var writerBlock = new ActionBlock<StringBuffer>(buffer =>
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
    /// Generates lines and returns the string buffer & size of the generated block.
    /// Note: the result size could be slightly bigger than required, but it's not important for this task.
    /// </summary>
    private static (StringBuffer, long) GenerateBuffer(ILineGenerator lineGenerator, long toGenerate)
    {
      using(Markers.EnterSpan("Test data generation."))
      {
        // Take the next line, add its length + 2 for line end
        // Repeat until we reach the required buffer size.
        var finalLength = 0;

        var lines = lineGenerator
          .GenerateLines()
          .TakeUntil(s =>
          {
            finalLength += s.Length + 2;
            return finalLength > toGenerate;
          })
          .ToArray();

        var stringBuffer = new StringBuffer(lines, lines.Length);
        return (stringBuffer, finalLength);
      }
    }

    private static void SaveBuffer(StringBuffer buffer, StreamWriter streamWriter)
    {
      using(Markers.EnterSpan("Saving data buffer"))
      {
        for(int i = 0; i < buffer.BufferSize; i++)
        {
          streamWriter.WriteLine(buffer.Buffer[i]);
        }
      }
    }
  }
}
