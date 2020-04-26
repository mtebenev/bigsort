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
    private readonly Dictionary<uint, List<SortRecord>> _buckets;
    private readonly IPipelineContext _pipelineContext;
    private readonly ILogger _logger;
    private readonly int _bufferSize;

    /// <summary>
    /// Ctor.
    /// </summary>
    private StringBufferBlock(IPipelineContext pipelineContext, int bufferSize)
    {
      this._bufferSize = bufferSize;
      this._buckets = new Dictionary<uint, List<SortRecord>>();
      this._pipelineContext = pipelineContext;
      this._logger = pipelineContext.LoggerFactory.CreateLogger(nameof(Blocks.StringBufferBlock));
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static TransformManyBlock<BufferReadEvent, SortBucket> Create(IPipelineContext pipelineContext, int bufferSize)
    {
      var block = new StringBufferBlock(pipelineContext, bufferSize);
      var result = new TransformManyBlock<BufferReadEvent, SortBucket>(
        (evt) => block.Execute(evt),
        new ExecutionDataflowBlockOptions
        {
          // BoundedCapacity is essential to block the reader until we process current messages,
          // otherwise the memory usage grows because the reader continues producing data.
          // Setting the bound capacity we are effectively blocking the reading to let us 'swallow' the current data.
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
      var flushedRecords = this.FlushChunks(evt.IsReadCompleted);

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
    /// </summary>
    private IEnumerable<SortBucket> FlushChunks(bool isReadingCompleted)
    {
      // TODO: this is not precise enough. The bucket threshold depends on the buckets count.
      // So the idea is that we should avoid memory paging. In the same time we should keep buckets big enough to reduce the merge sources.
      var maxBucketRecords = this._bufferSize / 10;
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
