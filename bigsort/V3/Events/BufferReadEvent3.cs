using System;
using System.Buffers;
using System.Collections.Generic;
using BigSort.Common;

namespace BigSort.V3.Events
{
  /// <summary>
  /// Sent when a block of memory with strings has been read from the source file.
  /// </summary>
  internal class BufferReadEvent3 : IDisposable
  {
    private readonly IMemoryOwner<byte> _memoryOwner;
    private readonly int _length;

    /// <summary>
    /// Ctor.
    /// </summary>
    public BufferReadEvent3(IMemoryOwner<byte> memoryOwner, int length, bool isReadCompleted)
    {
      this._memoryOwner = memoryOwner;
      this._length = length;
      this.IsReadCompleted = isReadCompleted;
    }

    public IEnumerable<string> EnumerateStrings()
    {
      return BufferStringEnumerator.EnumerateString(this._memoryOwner.Memory, this._length);
    }

    /// <summary>
    /// True when the block is final. This is the hint that the chunk merge could start.
    /// </summary>
    public bool IsReadCompleted { get; }

    /// <summary>
    /// IDisposable.
    /// </summary>
    public void Dispose()
    {
      this._memoryOwner.Dispose();
    }
  }
}
