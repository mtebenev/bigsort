using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BigSort.Common;
using StackExchange.Profiling;

namespace BigSort.V1
{
  /// <summary>
  /// Merge sort V1 (classic).
  /// </summary>
  internal class MergeSortTaskV1 : IMergeSortTask
  {
    private readonly IList<Task> _sortTasks;
    private readonly ConcurrentBag<string> _splitFilePaths;
    private readonly BlockingCollection<SortBuffer> _preSortQueue;
    private readonly BlockingCollection<SortBuffer> _writeChunkQueue;

    public MergeSortTaskV1()
    {
      this._sortTasks = new List<Task>();
      this._splitFilePaths = new ConcurrentBag<string>();
      this._preSortQueue = new BlockingCollection<SortBuffer>(6);
      this._writeChunkQueue = new BlockingCollection<SortBuffer>(1);
    }

    /// <summary>
    /// IMergeSortTask.
    /// </summary>
    public async Task ExecuteAsync(MergeSortOptions options)
    {
      using(MiniProfiler.Current.Step("Split step"))
      {
        Console.WriteLine("Splitting file...");

        // Split blocks consumer.
        var splitChunksTask = Task.Run(async () =>
        {
          while(!this._preSortQueue.IsCompleted)
          {
            try
            {
              // Parallel sort count
              if(this._sortTasks.Count < 7)
              {
                var splitBuffer = this._preSortQueue.Take();
                this.RunSortTask(splitBuffer);
              }
              else
              {
                var completedTask = await Task.WhenAny(this._sortTasks);
                this._sortTasks.Remove(completedTask);
              }
            }
            catch(Exception)
            {
              throw;
            }
          }
        });

        // Sorted chunks consumer.
        var writeChunksTask = Task.Run(() =>
        {
          while(!this._writeChunkQueue.IsCompleted)
          {
            try
            {
              var chunkBuffer = this._writeChunkQueue.Take();
              this.FlushBuffer(chunkBuffer);
            }
            catch(Exception)
            {
              throw;
            }
          }
        });

        using(StreamReader sr = File.OpenText(options.InFilePath))
        {
          //var splitBufferSize = 1000000; // 19mb?
          //var splitBufferSize = 2000000; // 38mb
          var splitBufferSize = 6000000; // 113mb
          //var splitBufferSize = 9000000; // 170mb
          //var splitBufferSize = 12000000; // 226mb
          var memBuffer = new string[splitBufferSize];

          string s = String.Empty;
          var splitBufferPos = 0;
          while((s = sr.ReadLine()) != null)
          {
            // Push the buffer to processing.
            // This may block the thread if we have too many concurrent sorting tasks.
            if(splitBufferPos == splitBufferSize)
            {
              var splitBuffer = new SortBuffer(memBuffer, splitBufferSize);
              this._preSortQueue.Add(splitBuffer);
              memBuffer = new string[splitBufferSize];
              splitBufferPos = 0;
            }
            memBuffer[splitBufferPos] = s;
            splitBufferPos++;
          }

          // Sort the final buffer
          if(splitBufferPos > 0)
          {
            var splitBuffer = new SortBuffer(memBuffer, splitBufferPos);
            this._preSortQueue.Add(splitBuffer);
          }
          this._preSortQueue.CompleteAdding();
          await splitChunksTask;
          await Task.WhenAll(this._sortTasks);
          this._writeChunkQueue.CompleteAdding();
          await writeChunksTask;
        }
      }

      using(MiniProfiler.Current.Step("Merge step"))
      {
        Console.WriteLine("Merging file...");
        this.MergeFiles(this._splitFilePaths.ToList());
      }
    }

    /// <summary>
    /// Executes sorting on the run.
    /// </summary>
    private void RunSortTask(SortBuffer buffer)
    {
      var sortTask = Task.Factory.StartNew(() =>
      {
        using(MiniProfiler.Current.CustomTiming("Sort chunk data", ""))
        {
          Console.WriteLine($"Running sort task. Currently: {this._sortTasks.Count}");
          Array.Sort(buffer.Buffer);
          this._writeChunkQueue.Add(buffer);
        }
      }, TaskCreationOptions.LongRunning);

      this._sortTasks.Add(sortTask);
    }

    private void MergeFiles(IList<string> filePaths)
    {
      var streams = filePaths
        .Select(fp => new FileStream(fp, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        .ToList();

      var readers = streams
        .Select(s => new StreamReader(s))
        .ToList();

      var outFileName = $@"c:\_sorting\out.txt";
      if(File.Exists(outFileName))
      {
        File.Delete(outFileName);
      }
      var outStream = new FileStream(outFileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
      StreamWriter sw = new StreamWriter(outStream);

      var heap = new C5.IntervalHeap<MergeHeapNode>(new MergeHeapNodeComparer());

      for(int i = 0; i < readers.Count; i++)
      {
        var s = readers[i].ReadLine();
        var node = new MergeHeapNode(i, s);
        heap.Add(node);
      }

      while(!heap.IsEmpty)
      {
        var node = heap.DeleteMin();
        sw.WriteLine(node.Data);

        var s = readers[node.FileIndex].ReadLine();
        if(s != null)
        {
          var nextNode = new MergeHeapNode(node.FileIndex, s);
          heap.Add(nextNode);
        }
      }

      sw.Flush();
      sw.Dispose();
      outStream.Dispose();
    }

    /// <summary>
    /// Flushes the sort buffer.
    /// </summary>
    private void FlushBuffer(SortBuffer buffer)
    {
      try
      {
        var chunkFilePath = this.SaveChunkFile(buffer);
        this._splitFilePaths.Add(chunkFilePath);
      }
      catch(Exception e)
      {
        throw;
      }
    }

    /// <summary>
    /// Saves the string buffer to a temporary file and returns its name.
    /// </summary>
    private string SaveChunkFile(SortBuffer buffer)
    {
      var chunkFilePath = $@"c:\_sorting\chunks\{new Random().Next()}.txt";

      using(MiniProfiler.Current.CustomTiming("Save chunk file", ""))
      {
        using(var stream = new FileStream(chunkFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
          using(StreamWriter sw = new StreamWriter(stream))
          {
            for(int i = 0; i < buffer.BufferSize; i++)
            {
              sw.WriteLine(buffer.Buffer[i]);
            }
            stream.Flush();
          }
        }
      }

      Console.WriteLine("Saved chunk file.");
      return chunkFilePath;
    }
  }
}
