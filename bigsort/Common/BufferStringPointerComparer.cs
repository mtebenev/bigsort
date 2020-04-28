using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BigSort.Common
{
  /// <summary>
  /// Compares two strings in buffer by the pointers.
  /// </summary>
  internal class BufferStringPointerComparer : IComparer<BufferStringPointer>
  {
    private readonly ReadOnlyMemory<byte> _memory;

    public BufferStringPointerComparer(ReadOnlyMemory<byte> memory)
    {
      this._memory = memory;
    }

    /// <summary>
    /// IComparable.
    /// </summary>
    public int Compare([AllowNull] BufferStringPointer x, [AllowNull] BufferStringPointer y)
    {
      var spanString1 = this._memory.Span.Slice(x.Start + x.DotPos + 2, x.Length - x.DotPos - 2);
      var spanString2 = this._memory.Span.Slice(y.Start + y.DotPos + 2, y.Length - y.DotPos - 2);

      var result = spanString1.SequenceCompareTo(spanString2);
      if(result == 0)
      {
        var spanNum1 = this._memory.Span.Slice(x.Start, x.DotPos);
        var spanNum2 = this._memory.Span.Slice(y.Start, y.DotPos);
        if(spanNum1.Length == spanNum2.Length)
        {
          for(int i = 0; i < spanNum1.Length; i++)
          {
            var n1 = spanNum1[i] - '0';
            var n2 = spanNum2[i] - '0';
            if(n1 > n2)
            {
              result = 1;
              break;
            }
            else if(n1 < n2)
            {
              result = -1;
              break;
            }
          }
        }
        else
        {
          result = spanNum1.Length > spanNum2.Length ? 1 : -1;
        }
      }
      return result;
    }
  }
}
