﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using Microsoft.ConcurrencyVisualizer.Instrumentation;

namespace BigSort.V2
{
  /// <summary>
  /// Responsible for merging individual buckets.
  /// </summary>
  internal class BucketMergeBlock
  {
    private readonly string _tempDirectoryPath;

    /// <summary>
    /// Ctor.
    /// </summary>
    private BucketMergeBlock(string tempDirectoryPath)
    {
      this._tempDirectoryPath = tempDirectoryPath;
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static IPropagatorBlock<BucketFlushEvent[], BucketMergeEvent> Create(MergeSortOptions options)
    {
      var block = new BucketMergeBlock(options.TempDirectoryPath);
      var result = new TransformBlock<BucketFlushEvent[], BucketMergeEvent>(
        (evt) => block.Execute(evt),
        new ExecutionDataflowBlockOptions
        {
          MaxDegreeOfParallelism = options.MaxConcurrentJobs
        });

      return result;
    }

    private BucketMergeEvent Execute(BucketFlushEvent[] events)
    {
      BucketMergeEvent result;
      Console.WriteLine($"BucketMergeBlock.Execute(). Files: {events.Length}");

      using(Markers.EnterSpan("Bucket merge"))
      {
        var chunkFilePath = Path.Combine(this._tempDirectoryPath, $@"{new Random().Next()}.txt");
        var filePaths = events
          .Select(e => e.FilePath)
          .ToList();

        BucketMerger.MergeAsyncKWay(filePaths, chunkFilePath);

        result = new BucketMergeEvent(chunkFilePath, events.First().Infix);
      }
      return result;
    }
  }
}