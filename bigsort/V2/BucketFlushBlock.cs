using System;
using System.IO;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using StackExchange.Profiling;

namespace BigSort.V2
{
  /// <summary>
  /// Responsible for flushing buckets
  /// </summary>
  internal class BucketFlushBlock
  {
    private readonly string _tempDirectoryPath;

    /// <summary>
    /// Ctor.
    /// </summary>
    private BucketFlushBlock(string tempDirectoryPath)
    {
      this._tempDirectoryPath = tempDirectoryPath;
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static TransformBlock<SortBucket, BucketFlushEvent> Create(MergeSortOptions options)
    {
      var block = new BucketFlushBlock(options.TempDirectoryPath);
      var result = new TransformBlock<SortBucket, BucketFlushEvent>(
        (bucket) => block.Execute(bucket));

      return result;
    }

    private BucketFlushEvent Execute(SortBucket bucket)
    {
      var chunkFilePath = Path.Combine(this._tempDirectoryPath, $@"{new Random().Next()}.txt");

      using(Markers.EnterSpan("Bucket flush"))
      using(MiniProfiler.Current.CustomTiming("Save chunk file", ""))
      using(var stream = new FileStream(chunkFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
      {
        using(StreamWriter sw = new StreamWriter(stream))
        {
          for(int i = 0; i < bucket.Records.Count; i++)
          {
            sw.WriteLine(bucket.Records[i].Value);
          }
          stream.Flush();
        }
      }
      
      Console.WriteLine("Saved chunk file.");
      
      var result = new BucketFlushEvent(chunkFilePath, bucket.Infix);
      return result;
    }
  }
}
