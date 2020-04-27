using System;
using System.Collections.Generic;
using System.Text;

namespace BigSort.Common
{
  /// <summary>
  /// Enumerates strings in byte buffer.
  /// Assumes \r\n line endings.
  /// Ignores empty lines (just line returns), but does not ignore the whitespace.
  /// </summary>
  internal static class BufferStringEnumerator
  {
    public static IEnumerable<string> EnumerateString(ReadOnlyMemory<byte> memory, int length)
    {
      var pos = 0;
      byte slashR = (byte)'\r';
      byte slashN = (byte)'\n';
      string s = String.Empty;
      var isNonEmpty = false;

      while(pos < length)
      {
        isNonEmpty = false;
        var workSpan = memory.Span.Slice(pos);
        var spanLenght = workSpan.Length;
        var idxR = workSpan.IndexOf(slashR);
        var isRN = idxR != -1 && idxR < spanLenght - 1 && workSpan[idxR + 1] == slashN;
        if(isRN)
        {
          if(idxR > 0)
          {
            s = Encoding.ASCII.GetString(workSpan.Slice(0, idxR));
            isNonEmpty = true;
          }
          pos += idxR + 2;
        }
        else
        {
          s = Encoding.ASCII.GetString(workSpan.Slice(0));
          pos += workSpan.Length;
          isNonEmpty = true;
        }
        
        if(isNonEmpty)
        {
          yield return s;
        }
      }
    }
  }
}
