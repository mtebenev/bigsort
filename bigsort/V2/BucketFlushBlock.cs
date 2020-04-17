using System.Threading.Tasks.Dataflow;

namespace BigSort.V2
{
  /// <summary>
  /// Responsible for flushing buckets
  /// </summary>
  internal class BucketFlushBlock
  {
    /// <summary>
    /// Ctor.
    /// </summary>
    private BucketFlushBlock()
    {
    }

    /// <summary>
    /// Factory.
    /// </summary>
    /// <returns></returns>
    public static ActionBlock<SortBucket> Create()
    {
      var block = new BucketFlushBlock();
      var result = new ActionBlock<SortBucket>(
        (bucket) => block.Execute(bucket));

      return result;
    }

    private void Execute(SortBucket bucket)
    {
      // Flushing...
    }
  }
}
