using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.V2;
using BigSort.V2.Events;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using Microsoft.Extensions.Logging;

namespace BigSort.Common
{
  /// <summary>
  /// The source data reader for V2.
  /// </summary>
  internal class SourceReader
  {
    /// <summary>
    /// Starts reading synchronously.
    /// <paramref name="blockSize">Number of lines in one block</paramref>
    /// </summary>
    public void Start(string inFilePath, int blockSize, IPipelineContext pipelineContext, ITargetBlock<BufferReadEvent> target)
    {
      var logger = pipelineContext.LoggerFactory.CreateLogger(nameof(SourceReader));
      logger.LogInformation("Started reading the source file.");
      var splitBufferSize = blockSize;
      using(var sr = File.OpenText(inFilePath))
      {
        var memBuffer = new string[splitBufferSize];

        var s = string.Empty;
        var splitBufferPos = 0;
        while((s = sr.ReadLine()) != null)
        {
          // Push the buffer to processing.
          // This may block the thread if we have too many concurrent sorting tasks.
          if(splitBufferPos == splitBufferSize)
          {
            var splitBuffer = new StringBuffer(memBuffer, splitBufferSize);
            var evt = new BufferReadEvent(splitBuffer, sr.EndOfStream);
            target.Post(evt);
            pipelineContext.Stats.AddBlockReads();
            logger.LogDebug("Pushed string buffer. size: {size}, final: {isFinal}", splitBufferSize, sr.EndOfStream);

            memBuffer = new string[splitBufferSize];
            splitBufferPos = 0;
          }
          memBuffer[splitBufferPos] = s;
          splitBufferPos++;
        }

        // Sort the final buffer
        if(splitBufferPos > 0)
        {
          var splitBuffer = new StringBuffer(memBuffer, splitBufferPos);
          var evt = new BufferReadEvent(splitBuffer, true);
          target.Post(evt);
          pipelineContext.Stats.AddBlockReads();
          logger.LogDebug("Pushed string buffer. size: {size}, final: {isFinal}", splitBufferPos, true);
        }

        target.Complete();

        // Visualizer
        Markers.WriteFlag("Reading completed.");
        logger.LogInformation("Reading completed.");
      }
    }

    /// <summary>
    /// Starts reading process asynchronously.
    /// </summary>
    public Task StartAsync(string inFilePath, int blockSize, IPipelineContext pipelineContext, ITargetBlock<BufferReadEvent> target)
    {
      return Task.Run(() =>
      {
        Start(inFilePath, blockSize, pipelineContext, target);
      });
    }
  }
}
