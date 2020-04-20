using System;
using System.IO;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// Responsible for flushing bucket chunks.
  /// </summary>
  internal class ChunkFlushBlock
  {
    private readonly string _tempDirectoryPath;
    private readonly ILogger _logger;

    /// <summary>
    /// Ctor.
    /// </summary>
    private ChunkFlushBlock(string tempDirectoryPath, IPipelineContext pipelineContext)
    {
      this._tempDirectoryPath = tempDirectoryPath;
      this._logger = pipelineContext.LoggerFactory.CreateLogger(nameof(Blocks.ChunkFlushBlock));
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static TransformBlock<SortBucket, ChunkFlushEvent> Create(MergeSortOptions options, IPipelineContext pipelineContext)
    {
      var block = new ChunkFlushBlock(options.TempDirectoryPath, pipelineContext);
      var result = new TransformBlock<SortBucket, ChunkFlushEvent>(
        (bucket) => block.Execute(bucket, pipelineContext));

      return result;
    }

    private ChunkFlushEvent Execute(SortBucket bucket, IPipelineContext pipelineContext)
    {
      this._logger.LogDebug("Flushing chunk, infix: {infix}", InfixUtils.InfixToString(bucket.Infix));

      var chunkFilePath = Path.Combine(this._tempDirectoryPath, $@"{new Random().Next()}.txt");

      using(Markers.EnterSpan("Bucket flush"))
      using(MiniProfiler.Current.CustomTiming("Save chunk file", ""))
      using(var stream = new FileStream(chunkFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
      {
        using(var sw = new StreamWriter(stream))
        {
          for(var i = 0; i < bucket.Records.Count; i++)
          {
            sw.WriteLine(bucket.Records[i].Value);
          }
          stream.Flush();
        }
      }

      pipelineContext.Stats.AddChunkFlushes();
      pipelineContext.OnChunkFlush(bucket.Infix);

      var result = new ChunkFlushEvent(chunkFilePath, bucket.Infix);
      return result;
    }
  }
}
