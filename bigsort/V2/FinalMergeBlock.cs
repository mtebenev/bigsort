using System;
using System.Threading.Tasks.Dataflow;

namespace BigSort.V2
{
  /// <summary>
  /// Performs the final merge.
  /// </summary>
  internal class FinalMergeBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static ITargetBlock<BucketMergeEvent[]> Create()
    {
      var result = new ActionBlock<BucketMergeEvent[]>(events =>
      {
        Console.WriteLine($"FinalMergeBlock.Execute(). Files: ");
        for(int i = 0; i < events.Length; i++)
        {
          Console.WriteLine($"File {i}: {events[i].FilePath}");
        }
      });

      return result;
    }
  }
}
