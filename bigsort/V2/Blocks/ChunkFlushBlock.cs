using System.IO;
using System.Threading.Tasks.Dataflow;
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
    private readonly ILogger _logger;

    /// <summary>
    /// Ctor.
    /// </summary>
    private ChunkFlushBlock(IPipelineContext pipelineContext)
    {
      this._logger = pipelineContext.LoggerFactory.CreateLogger(nameof(Blocks.ChunkFlushBlock));
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static TransformBlock<SortBucket, ChunkFlushEvent> Create(IPipelineContext pipelineContext)
    {
      var block = new ChunkFlushBlock(pipelineContext);
      var result = new TransformBlock<SortBucket, ChunkFlushEvent>(
        (bucket) => block.Execute(bucket, pipelineContext),
        new ExecutionDataflowBlockOptions 
        {
          // One in, one out for the flushing
          MaxDegreeOfParallelism = 1,
          BoundedCapacity = 1
        });

      return result;
    }

    private ChunkFlushEvent Execute(SortBucket bucket, IPipelineContext pipelineContext)
    {
      this._logger.LogDebug("Flushing chunk, infix: {infix}", InfixUtils.InfixToString(bucket.Infix));

      var chunkFilePath = pipelineContext.FileContext.AddTempFile();

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
