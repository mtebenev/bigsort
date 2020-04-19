using System;
using System.IO;
using System.Threading.Tasks.Dataflow;
using BigSort.V2;
using BigSort.V2.Events;
using Microsoft.ConcurrencyVisualizer.Instrumentation;

namespace BigSort.Common
{
  /// <summary>
  /// The source data reader for V2.
  /// </summary>
  internal class SourceReader
  {
    public void Start(string inFilePath, IPipelineContext pipelineContext, ITargetBlock<BufferReadEvent> target)
    {
      using(var sr = File.OpenText(inFilePath))
      {
        var splitBufferSize = 1000000; // 19mb?
        //var splitBufferSize = 6000000; // 113mb
        var memBuffer = new string[splitBufferSize];

        var s = string.Empty;
        var splitBufferPos = 0;
        while((s = sr.ReadLine()) != null)
        {
          // Push the buffer to processing.
          // This may block the thread if we have too many concurrent sorting tasks.
          if(splitBufferPos == splitBufferSize)
          {
            var splitBuffer = new StringBuffer(memBuffer, splitBufferSize);
            var evt = new BufferReadEvent(splitBuffer, sr.EndOfStream);
            target.Post(evt);
            pipelineContext.AddBlockReads();

            memBuffer = new string[splitBufferSize];
            splitBufferPos = 0;
          }
          memBuffer[splitBufferPos] = s;
          splitBufferPos++;
        }

        // Sort the final buffer
        if(splitBufferPos > 0)
        {
          var splitBuffer = new StringBuffer(memBuffer, splitBufferPos);
          var evt = new BufferReadEvent(splitBuffer, true);
          target.Post(evt);
        }

        target.Complete();

        // Visualizer
        Markers.WriteFlag("Reading completed.");
        Console.WriteLine("Reading completed.");
      }
    }
  }
}
