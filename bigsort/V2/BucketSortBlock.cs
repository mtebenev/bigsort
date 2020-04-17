using System.Threading.Tasks.Dataflow;

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
    public static TransformBlock<SortBucket, SortBucket> Create()
    {
      var block = new BucketSortBlock();
      var result = new TransformBlock<SortBucket, SortBucket>(
        (bucket) => block.Execute(bucket));

      return result;
    }

    private SortBucket Execute(SortBucket bucket)
    {
      return bucket;
    }
  }
}
