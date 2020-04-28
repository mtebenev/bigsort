using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.V3.Events;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using Microsoft.Extensions.Logging;

namespace BigSort.Common
{
  /// <summary>
  /// The new source data reader for V3.
  /// </summary>
  internal class SourceReader3
  {
    /// <summary>
    /// Starts the reading.
    /// <paramref name="blockSize">Number of lines in one block</paramref>
    /// </summary>
    public async Task StartAsync(
      ILoggerFactory loggerFactory,
      string inFilePath, int blockSize,
      Stats stats,
      ITargetBlock<BufferReadEvent3> target)
    {
      var inFileSize = new FileInfo(inFilePath).Length;
      var progressCounter = new FileProgressCounter(inFileSize);
      var logger = loggerFactory.CreateLogger(nameof(SourceReader));
      logger.LogInformation("Started reading the source file.");
      var memoryBufferSize = (int)StringUtils.ParseFileSize("100mb", 1024); // Read by 100mb blocks
      using(var file = File.OpenRead(inFilePath))
      {
        var bytePool = MemoryPool<byte>.Shared;
        var byteBuffer = bytePool.Rent(memoryBufferSize);

        var isCompleted = false;
        do
        {
          var read = file.Read(byteBuffer.Memory.Span);
          isCompleted = read < memoryBufferSize;

          // Push the buffer to processing.
          // This may block the thread if we have too many concurrent sorting tasks.
          int spanEnd = read;
          if(!isCompleted)
          {
            spanEnd = this.FindLastLineEnd(byteBuffer.Memory, read);
            var toSeekBack = spanEnd - memoryBufferSize;
            file.Seek(toSeekBack, SeekOrigin.Current);
          }

          var evt = new BufferReadEvent3(byteBuffer, spanEnd, isCompleted);
          var sendResult = await target.SendAsync(evt);
          if(!sendResult)
          {
            throw new InvalidOperationException("Failed to push a data block into the pipeline.");
          }
          stats.AddBlockReads();
          logger.LogDebug("Pushed string buffer. size: {size}, final: {isFinal}, progress: {progress}", read, isCompleted, progressCounter.GetProgressText());

          byteBuffer = bytePool.Rent(memoryBufferSize);
        } while(!isCompleted);

        target.Complete();

        // Visualizer
        Markers.WriteFlag("Reading completed.");
        logger.LogInformation("Reading completed.");
      }
    }

    private int FindLastLineEnd(Memory<byte> memory, int length)
    {
      var pos = length - 1;
      var span = memory.Span;

      byte slashR = (byte)'\r';
      byte slashN = (byte)'\n';

      while(span[pos] != slashN && span[pos - 1] != slashR)
        pos--;

      return pos;
    }
  }
}
