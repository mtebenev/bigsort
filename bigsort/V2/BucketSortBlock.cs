using System.Threading.Tasks.Dataflow;
using BigSort.Common;

namespace BigSort.V2
{
  /// <summary>
  /// Responsible for sorting bucket content.
  /// </summary>
  internal class BucketSortBlock
  {
    /// <summary>
    /// Ctor.
    /// </summary>
    private BucketSortBlock()
    {
    }

    /// <summary>
    /// Factory.
    /// </summary>
    public static TransformBlock<SortBucket, SortBucket> Create(MergeSortOptions options)
    {
      var block = new BucketSortBlock();
      var result = new TransformBlock<SortBucket, SortBucket>(
        (bucket) => block.Execute(bucket),
        new ExecutionDataflowBlockOptions
        {
          MaxDegreeOfParallelism = options.MaxConcurrentJobs
        });

      return result;
    }

    private SortBucket Execute(SortBucket bucket)
    {
      var comparer = new SortRecordComparer();
      bucket.Records.Sort(comparer);
      return bucket;
    }
  }
}
