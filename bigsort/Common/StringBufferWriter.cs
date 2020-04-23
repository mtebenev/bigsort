using System;
using System.IO;

namespace BigSort.Common
{
  /// <summary>
  /// The utility class for writing string buffer into a stream.
  /// </summary>
  internal class StringBufferWriter
  {
    /// <summary>
    /// Writes the buffer line by line.
    /// </summary>
    public static void WriteBuffer(ReadOnlySpan<char> span, TextWriter writer)
    {
      var workSpan = span;
      while(workSpan.Length > 0)
      {
        var next = workSpan.IndexOf('\n');
        var ss = workSpan.Slice(0, next);
        writer.WriteLine(ss);

        workSpan = workSpan.Slice(next + 1);
      }
    }
  }
}
