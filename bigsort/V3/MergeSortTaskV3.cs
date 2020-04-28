using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2;
using Microsoft.Extensions.Logging;

namespace BigSort.V3
{
  /// <summary>
  /// The V3 implementation.
  /// Note: this is experimental. I'm just curious what a low-level file access can bing
  /// in terms of performance.
  /// </summary>
  internal class MergeSortTaskV3 : IMergeSortTask
  {
    /// <summary>
    /// IMergeSortTask.
    /// </summary>
    public async Task ExecuteAsync(IFileContext fileContext, ILoggerFactory loggerFactory, MergeSortOptions options)
    {
      var logger = loggerFactory.CreateLogger(nameof(MergeSortTaskV3));
      var bufferSize = this.EstimateBufferSize(logger, fileContext, options.MaxConcurrentJobs);

      var context = new PipelineContext(loggerFactory, fileContext);
      var reader = new SourceReader3();
      var (startBlock, finishBlock) = PipelineBuilder3.Build(options, context, bufferSize);

      var chunkPaths = new List<string>();
      var terminator = new ActionBlock<string>(chunkPath =>
      {
        chunkPaths.Add(chunkPath);
      });
      finishBlock.LinkTo(terminator, new DataflowLinkOptions { PropagateCompletion = true });

      var readerTask = reader.StartAsync(
        loggerFactory,
        fileContext.InFilePath,
        bufferSize,
        context.Stats,
        startBlock
      );

      await Task.WhenAll(readerTask, startBlock.Completion, terminator.Completion);
      context.Stats.PrintStats();

      // Done all
      foreach(var p in chunkPaths)
      {
        Console.WriteLine($"Chunk: {p}");
      }
    }

    /// <summary>
    /// Calculates buffer size for the merge sort. The considerations are:
    /// 1. We have N logical processors, we have M file size.
    /// 2. We will have N buffers processing (sorting) the data in the same time.
    /// 3. We don't need a lot of memory for merging (we are reading/writing files line by line).
    /// 4. We need a lot memory for sorting buffers (N) and grouping buffers (in-memory buckets).
    /// </summary>
    private int EstimateBufferSize(ILogger logger, IFileContext fileContext, int maxConcurrentJobs)
    {
      var result = 6000000; // The empiric value for small files.

      // Files smaller than 1gb probably should not be probed at all.
      // Let's assume that the performance is not critical for such files.
      var probingThresold = StringUtils.ParseFileSize("1gb", 1024);
      if(fileContext.GetInFileSize() > probingThresold)
      {
        // Let's use 2GB memory for base (sorting).
        var allowedMemory = StringUtils.ParseFileSize("2gb", 1024);
        var bufferVolume = allowedMemory / maxConcurrentJobs;

        // Read first 1000 lines of the file and get average line length.
        // Estimate the buffer size from this average length.
        var averageLineLength = File
          .ReadLines(fileContext.InFilePath)
          .Take(1000)
          .Select(s => s.Length + 1)
          .Average();

        // Average lines per buffer.
        result = (int)(bufferVolume / averageLineLength);

        logger.LogInformation($"Probing file completed. Estimated buffer size: {StringUtils.GetHumanReadableSize((long)bufferVolume)}, in lines: {result}");
      }

      return result;
    }
  }
}
