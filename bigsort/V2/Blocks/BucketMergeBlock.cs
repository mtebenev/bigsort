using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using Microsoft.Extensions.Logging;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// Responsible for merging individual buckets.
  /// </summary>
  internal class BucketMergeBlock
  {
    private readonly IPipelineContext _pipelineContext;
    private readonly ILogger _logger;

    /// <summary>
    /// Ctor.
    /// </summary>
    private BucketMergeBlock(IPipelineContext pipelineContext)
    {
      this._pipelineContext = pipelineContext;
      this._logger = pipelineContext.LoggerFactory.CreateLogger(nameof(Blocks.BucketMergeBlock));
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<ChunkFlushEvent[], BucketMergeEvent> Create(MergeSortOptions options, IPipelineContext pipelineContext)
    {
      var block = new BucketMergeBlock(pipelineContext);
      var result = new TransformBlock<ChunkFlushEvent[], BucketMergeEvent>(
        (evt) => block.Execute(evt),
        new ExecutionDataflowBlockOptions
        {
          MaxDegreeOfParallelism = options.MaxConcurrentJobs
        });

      return result;
    }

    private BucketMergeEvent Execute(ChunkFlushEvent[] events)
    {
      this._logger.LogDebug("Merging bucket, infix: {infix}", InfixUtils.InfixToString(events.First().Infix));
      BucketMergeEvent result;

      using(Markers.EnterSpan("Bucket merge"))
      {
        var chunkFilePath = this._pipelineContext.FileContext.AddTempFile();
        var filePaths = events
          .Select(e => e.FilePath)
          .ToList();

        BucketMerger.MergeKWay(filePaths, chunkFilePath);
        this._pipelineContext.Stats.AddBucketMerges();

        result = new BucketMergeEvent(chunkFilePath, events.First().Infix);
      }
      return result;
    }
  }
}
