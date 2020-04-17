using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace BigSort.V2
{
  /// <summary>
  /// Responsible for merging individual buckets.
  /// </summary>
  internal class BucketMergeBlock
  {
    /// <summary>
    /// Ctor.
    /// </summary>
    private BucketMergeBlock()
    {
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<BucketFlushEvent[], BucketMergeEvent> Create()
    {
      var block = new BucketMergeBlock();
      var result = new TransformBlock<BucketFlushEvent[], BucketMergeEvent>(
        (evt) => block.Execute(evt));

      return result;
    }

    private BucketMergeEvent Execute(BucketFlushEvent[] events)
    {
      Console.WriteLine($"BucketMergeBlock.Execute(). Files: ");
      for(int i = 0; i < events.Length; i++)
      {
        Console.WriteLine($"File {i}: {events[i].FilePath}");
      }

      var result = new BucketMergeEvent("some-path", events.First().Infix);
      return result;
    }
  }
}
