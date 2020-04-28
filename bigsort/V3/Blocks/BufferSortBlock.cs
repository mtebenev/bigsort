using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2;
using BigSort.V3.Events;

namespace BigSort.V3.Blocks
{
  /// <summary>
  /// Sorts strings in the buffer (actually sort the pointers).
  /// </summary>
  internal static class BufferSortBlock
  {
    public static TransformBlock<BufferReadEvent3, string> Create(IPipelineContext pipelineContext)
    {
      var result = new TransformBlock<BufferReadEvent3, string>(evt =>
      {
        string outFileName;
        try
        {
          var pointers = BufferStringEnumerator
          .EnumeratePointers(evt.Memory)
          .ToArray();

          var comparer = new BufferStringPointerComparer(evt.Memory);
          Array.Sort(pointers, comparer);

          Console.WriteLine("Done sorting.");
          outFileName = pipelineContext.FileContext.AddTempFile("chunk");

          ChunkReadWrite.Write(new FileSystem(), outFileName, evt.Memory);

          using(var file = File.OpenWrite(outFileName))
          {
            var bytesRn = new byte[] { (byte)'\r', (byte)'\n' };
            for(int i = 0; i < pointers.Length; i++)
            {
              file.Write(evt.Memory.Span.Slice(pointers[i].Start, pointers[i].Length));
              file.Write(bytesRn);
            }

            file.Flush();
            file.Close();
          }
        }
        finally
        {
          evt.Dispose();
        }

        return outFileName;
      },
      new ExecutionDataflowBlockOptions
      {
        BoundedCapacity = 8,
        MaxDegreeOfParallelism = 8
      });

      return result;
    }
  }
}
