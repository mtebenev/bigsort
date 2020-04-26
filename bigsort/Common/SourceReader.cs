using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
    /// Starts the reading.
    /// <paramref name="blockSize">Number of lines in one block</paramref>
    /// </summary>
    public async Task StartAsync(
      ILoggerFactory loggerFactory,
      string inFilePath, int blockSize,
      Stats stats,
      ITargetBlock<BufferReadEvent> target)
    {
      var inFileSize = new FileInfo(inFilePath).Length;
      var progressCounter = new FileProgressCounter(inFileSize);
      var logger = loggerFactory.CreateLogger(nameof(SourceReader));
      logger.LogInformation("Started reading the source file.");
      var splitBufferSize = blockSize;
      using(var sr = File.OpenText(inFilePath))
      {
        var stringBuffer = StringBuffer.Allocate(splitBufferSize);

        var s = string.Empty;
        var splitBufferPos = 0;
        while((s = sr.ReadLine()) != null)
        {
          // Push the buffer to processing.
          // This may block the thread if we have too many concurrent sorting tasks.
          if(splitBufferPos == splitBufferSize)
          {
            var evt = new BufferReadEvent(stringBuffer, sr.EndOfStream);
            var sendResult = await target.SendAsync(evt);
            if(!sendResult)
            {
              throw new InvalidOperationException("Failed to push a data block into the pipeline.");
            }
            stats.AddBlockReads();
            logger.LogDebug("Pushed string buffer. size: {size}, final: {isFinal}, progress: {progress}", splitBufferSize, sr.EndOfStream, progressCounter.GetProgressText());

            stringBuffer = StringBuffer.Allocate(splitBufferSize);
            splitBufferPos = 0;
          }
          stringBuffer.Buffer[splitBufferPos] = s;
          stringBuffer.ActualSize += 1;
          splitBufferPos++;
          progressCounter.OnLineProcessed(s);
        }

        // Sort the final buffer
        if(splitBufferPos > 0)
        {
          var evt = new BufferReadEvent(stringBuffer, true);
          var sendResult = await target.SendAsync(evt);
          if(!sendResult)
          {
            throw new InvalidOperationException("Failed to push a data block into the pipeline.");
          }
          stats.AddBlockReads();
          logger.LogDebug("Pushed string buffer. size: {size}, final: {isFinal}", splitBufferPos, true);
        }

        target.Complete();

        // Visualizer
        Markers.WriteFlag("Reading completed.");
        logger.LogInformation("Reading completed.");
      }
    }
  }
}
