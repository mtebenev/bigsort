using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace BigSort.V2
{
  /// <summary>
  /// The block is responsible for bucket collecting records into the buckets.
  /// </summary>
  internal class BucketBufferBlock
  {
    private readonly Dictionary<long, List<SortRecord>> _buckets;

    /// <summary>
    /// Ctor.
    /// </summary>
    private BucketBufferBlock()
    {
      this._buckets = new Dictionary<long, List<SortRecord>>();
    }

    /// <summary>
    /// Factory.
    /// </summary>
    public static TransformManyBlock<StringBuffer, SortBucket> Create()
    {
      var block = new BucketBufferBlock();
      var result = new TransformManyBlock<StringBuffer, SortBucket>(
        (stringBuffer) => block.Execute(stringBuffer));

      return result;
    }

    /// <summary>
    /// Executes the bucket sort on the string buffer.
    /// </summary>
    public IEnumerable<SortBucket> Execute(StringBuffer stringBuffer)
    {
      this.PushNewRecords(stringBuffer);
      var flushedRecords = this.FlushBuckets();

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
    private IEnumerable<SortBucket> FlushBuckets()
    {
      var maxBucketRecords = 10000;
      var flushedBuckets = this._buckets
        .Where(kvp => kvp.Value.Count > maxBucketRecords)
        .Select(kvp => new SortBucket(kvp.Key, kvp.Value))
        .ToList();

      foreach(var fb in flushedBuckets)
      {
        this._buckets.Remove(fb.Infix);
      }

      return flushedBuckets;
    }
  }
}
