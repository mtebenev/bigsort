using System;
using System.Buffers;

namespace BigSort.V2
{
  /// <summary>
  /// The fixed-size sort records array.
  /// </summary>
  internal class SortRecordBuffer : IDisposable
  {
    private SortRecord[] _buffer;
    private int _bufferSize;

    /// <summary>
    /// Ctor.
    /// </summary>
    private SortRecordBuffer(SortRecord[] buffer, int bufferSize)
    {
      this._buffer = buffer;
      this._bufferSize = bufferSize;
    }

    /// <summary>
    /// Factory method. Use to allocate the buffer of required size.
    /// </summary>
    public static SortRecordBuffer Allocate(int bufferSize)
    {
      var pool = ArrayPool<SortRecord>.Shared;
      var memBuffer = pool.Rent(bufferSize);

      var result = new SortRecordBuffer(memBuffer, bufferSize);
      return result;
    }

    /// <summary>
    /// Updates size position of the buffer.
    /// </summary>
    public void SetBufferSize(int size)
    {
      this._bufferSize = size;
    }

    /// <summary>
    /// IDisposable.
    /// </summary>
    public void Dispose()
    {
      var pool = ArrayPool<SortRecord>.Shared;
      pool.Return(this.Buffer);
      this._buffer = null;
    }

    /// <summary>
    /// The buffer.
    /// </summary>
    public SortRecord[] Buffer => this._buffer;

    /// <summary>
    /// Actual buffer size.
    /// </summary>
    public int BufferSize => this._bufferSize;

  }
}
