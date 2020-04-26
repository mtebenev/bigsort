using System;
using System.Buffers;

namespace BigSort.Common
{
  /// <summary>
  /// Keeps strings for processing. Really used number of strings is BufferSize.
  /// We pre-allocate the string buffers.
  /// </summary>
  public class StringBuffer : IDisposable
  {
    private string[] _buffer;
    private bool _isDisposed;

    /// <summary>
    /// Ctor.
    /// </summary>
    private StringBuffer(string[] buffer)
    {
      this._buffer = buffer;
      this._isDisposed = false;
    }

    public static StringBuffer Allocate(int bufferSize)
    {
      var pool = ArrayPool<string>.Shared;
      var memBuffer = pool.Rent(bufferSize);

      var result = new StringBuffer(memBuffer);
      return result;
    }

    /// <summary>
    /// The actual buffer.
    /// </summary>
    public string[] Buffer => this._buffer;

    /// <summary>
    /// The actual buffer size (number of filled strings).
    /// </summary>
    public int ActualSize { get; set; }

    public void Dispose()
    {
      if(this._isDisposed)
      {
        throw new InvalidOperationException("Attempted to call Dispose() on already disposed string buffer.");
      }

      var pool = ArrayPool<string>.Shared;
      pool.Return(this._buffer);
      this._buffer = null;
    }
  }
}
