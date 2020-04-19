using System;
using System.IO;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using StackExchange.Profiling;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// Responsible for flushing bucket chunks.
  /// </summary>
  internal class ChunkFlushBlock
  {
    private readonly string _tempDirectoryPath;

    /// <summary>
    /// Ctor.
    /// </summary>
    private ChunkFlushBlock(string tempDirectoryPath)
    {
      this._tempDirectoryPath = tempDirectoryPath;
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static TransformBlock<SortBucket, ChunkFlushEvent> Create(MergeSortOptions options, IPipelineContext pipelineContext)
    {
      var block = new ChunkFlushBlock(options.TempDirectoryPath);
      var result = new TransformBlock<SortBucket, ChunkFlushEvent>(
        (bucket) => block.Execute(bucket, pipelineContext),
        new ExecutionDataflowBlockOptions { EnsureOrdered = true });

      return result;
    }

    private ChunkFlushEvent Execute(SortBucket bucket, IPipelineContext pipelineContext)
    {
      // TODOA: diagnostics
      if(pipelineContext.IsBucketFlushed(bucket.Infix))
      {
        throw new NotImplementedException();
      }

      var chunkFilePath = Path.Combine(this._tempDirectoryPath, $@"{new Random().Next()}.txt");

      using(Markers.EnterSpan("Bucket flush"))
      using(MiniProfiler.Current.CustomTiming("Save chunk file", ""))
      using(var stream = new FileStream(chunkFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
      {
        using(var sw = new StreamWriter(stream))
        {
          for(var i = 0; i < bucket.Records.Count; i++)
          {
            sw.WriteLine(bucket.Records[i].Value);
          }
          stream.Flush();
        }
      }

      if(bucket.IsFinalChunk)
      {
        pipelineContext.SetBucketFlushed(bucket.Infix);
      }
      pipelineContext.AddChunkFlushes();

      var result = new ChunkFlushEvent(chunkFilePath, bucket.Infix, bucket.IsFinalChunk);
      return result;
    }
  }
}
