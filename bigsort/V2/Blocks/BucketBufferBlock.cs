using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// The block is responsible for bucket collecting records into the buckets.
  /// </summary>
  internal class BucketBufferBlock
  {
    private readonly Dictionary<long, List<SortRecord>> _buckets;
    private readonly IPipelineContext _pipelineContext;

    /// <summary>
    /// Ctor.
    /// </summary>
    private BucketBufferBlock(IPipelineContext pipelineContext)
    {
      this._buckets = new Dictionary<long, List<SortRecord>>();
      this._pipelineContext = pipelineContext;
    }

    /// <summary>
    /// The factory.
    /// TODOA: comment options
    /// </summary>
    public static TransformManyBlock<BufferReadEvent, SortBucket> Create(IPipelineContext pipelineContext)
    {
      var block = new BucketBufferBlock(pipelineContext);
      var result = new TransformManyBlock<BufferReadEvent, SortBucket>(
        (evt) => block.Execute(evt),
        new ExecutionDataflowBlockOptions
        {
          MaxDegreeOfParallelism = 1,
          BoundedCapacity = 1,
          EnsureOrdered = true
        });
      result.Completion.ContinueWith(t =>
        {
          Console.WriteLine($"Completing buffer. Input count: {result.InputCount}, output count: {result.OutputCount}");
        },
        default,
        TaskContinuationOptions.ExecuteSynchronously,
        TaskScheduler.Default
      );

      return result;
    }

    /// <summary>
    /// Executes the bucket sort on the string buffer.
    /// </summary>
    public IEnumerable<SortBucket> Execute(BufferReadEvent evt)
    {
      this.PushNewRecords(evt.Buffer);
      var flushedRecords = this.FlushBuckets(evt.IsReadCompleted);
      Console.WriteLine($"Buffer: strings -> records. Reading completed: {this._pipelineContext.IsReadingCompleted}");

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
        .Select(kvp => new SortBucket(kvp.Key, kvp.Value, isReadingCompleted))
        .ToList();

      foreach(var fb in flushedBuckets)
      {
        this._buckets.Remove(fb.Infix);
      }

      return flushedBuckets;
    }
  }
}
