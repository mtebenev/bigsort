using System;

namespace BigSort.V2
{
  /// <summary>
  /// Wraps the sort records array along with the infix.
  /// </summary>
  internal class SortChunkBuffer : IDisposable
  {
    private SortRecordBuffer _sortRecordBuffer;

    /// <summary>
    /// Ctor.
    /// </summary>
    public SortChunkBuffer(SortRecordBuffer sortRecordBuffer, uint Infix)
    {
      this._sortRecordBuffer = sortRecordBuffer;
      this.Infix = Infix;
    }

    /// <summary>
    /// The wrapped buffer.
    /// </summary>
    public SortRecordBuffer SortRecordBuffer => this._sortRecordBuffer;

    /// <summary>
    /// The infix.
    /// </summary>
    public uint Infix { get; }

    /// <summary>
    /// IDisposable.
    /// </summary>
    public void Dispose()
    {
      this._sortRecordBuffer.Dispose();
      this._sortRecordBuffer = null;
    }
  }
}
