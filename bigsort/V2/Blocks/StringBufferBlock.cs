using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;
using Microsoft.Extensions.Logging;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// The block is responsible for bucket collecting records into the buckets.
  /// </summary>
  internal class StringBufferBlock
  {
    private readonly Dictionary<long, List<SortRecord>> _buckets;
    private readonly IPipelineContext _pipelineContext;
    private readonly ILogger _logger;

    /// <summary>
    /// Ctor.
    /// </summary>
    private StringBufferBlock(IPipelineContext pipelineContext)
    {
      this._buckets = new Dictionary<long, List<SortRecord>>();
      this._pipelineContext = pipelineContext;
      this._logger = pipelineContext.LoggerFactory.CreateLogger(nameof(Blocks.StringBufferBlock));
    }

    /// <summary>
    /// The factory.
    /// TODOA: comment options
    /// </summary>
    public static TransformManyBlock<BufferReadEvent, SortBucket> Create(IPipelineContext pipelineContext)
    {
      var block = new StringBufferBlock(pipelineContext);
      var result = new TransformManyBlock<BufferReadEvent, SortBucket>(
        (evt) => block.Execute(evt),
        new ExecutionDataflowBlockOptions
        {
          // BoundCapacity is essential to block the reader until we process current messages.
          // Otherwise the memory usage grows because reader continues producing data.
          BoundedCapacity = 2,
          MaxDegreeOfParallelism = 1
        });

      return result;
    }

    /// <summary>
    /// Executes the bucket sort on the string buffer.
    /// </summary>
    public IEnumerable<SortBucket> Execute(BufferReadEvent evt)
    {
      this._logger.LogInformation("Start string buffer processing.");
      this.PushNewRecords(evt.Buffer);
      var flushedRecords = this.FlushBuckets(evt.IsReadCompleted);

      this._logger.LogInformation("Finished string buffer processing.");
      return flushedRecords;
    }

    /// <summary>
    /// Pushes all newly obtained records the buckets.
    /// </summary>
    private void PushNewRecords(StringBuffer stringBuffer)
    {
      var sortRecords = stringBuffer
        .Buffer
        .Take(stringBuffer.BufferSize)
        .Select(s => new SortRecord(s));

      foreach(var sr in sortRecords)
      {
        List<SortRecord> recordList;
        if(!this._buckets.TryGetValue(sr.Infix, out recordList))
        {
          recordList = new List<SortRecord>();
          this._buckets[sr.Infix] = recordList;
        }

        recordList.Add(sr);
      }
    }

    /// <summary>
    /// Flushes full buckets if any.
    /// TODOA: confusing name
    /// </summary>
    private IEnumerable<SortBucket> FlushBuckets(bool isReadingCompleted)
    {
      var maxBucketRecords = 10000;
      var stringSource = isReadingCompleted
        ? this._buckets
        : this._buckets.Where(kvp => kvp.Value.Count > maxBucketRecords);

      var flushedBuckets = stringSource
        .Select(kvp => new SortBucket(kvp.Key, kvp.Value))
        .ToList();

      foreach(var fb in flushedBuckets)
      {
        this._buckets.Remove(fb.Infix);
        this._pipelineContext.OnChunkStart(fb.Infix);
        this._logger.LogDebug("Flushed block, infix: {infix}", InfixUtils.InfixToString(fb.Infix));
      }

      if(isReadingCompleted)
      {
        this._pipelineContext.OnInfixesReady();
      }

      return flushedBuckets;
    }
  }
}
