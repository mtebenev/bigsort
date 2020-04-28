using System;
using System.IO;
using System.IO.Abstractions;
using BigSort.Common;

namespace BigSort.V3
{
  /// <summary>
  /// Helper class for serializing chunks in a 'binary' form.
  /// </summary>
  internal static class ChunkReadWrite
  {
    private struct RecordHeader
    {
      public int StringLength;
      public int DotPos;
    }

    public static void Write(IFileSystem fileSystem, string outPath, ReadOnlyMemory<byte> memory)
    {
      var header = new RecordHeader();

      using(var file = fileSystem.File.OpenWrite(outPath))
      using(var bs = new BufferedStream(file, 4096))
      {
        var source = BufferStringEnumerator.EnumeratePointers(memory);
        foreach(var p in source)
        {
          unsafe
          {
            header.StringLength = p.Length;
            header.DotPos = p.DotPos;
            var pointerSpan = new Span<byte>(&header, sizeof(BufferStringPointer));
            bs.Write(pointerSpan);
          }
          var stringSpan = memory.Span.Slice(p.Start, p.Length);
          bs.Write(stringSpan);
        }

        bs.Flush();
        file.Flush();
      }
    }
  }
}
