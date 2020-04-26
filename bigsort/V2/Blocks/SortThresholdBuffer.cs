namespace BigSort.V2.Blocks
{
  /// <summary>
  /// The utility buffer used in the buffer block for bucket-divided buffers.
  /// Design note: although this class holds a reference to the IDisposable, the
  /// SortThresholdBuffer does not owner it. The inner SortChunkBuffer to be passed
  /// along the pipeline and disposed as soon as the data processed.
  /// </summary>
  internal class SortThresholdBuffer
  {
    private SortChunkBuffer _sortChunkBuffer;

    /// <summary>
    /// Ctor.
    /// </summary>
    private SortThresholdBuffer(SortChunkBuffer sortChunkBuffer)
    {
      this._sortChunkBuffer = sortChunkBuffer;
    }

    /// <summary>
    /// Allocates the buffer.
    /// </summary>
    public static SortThresholdBuffer Allocate(uint infix, int size)
    {
      var sortRecordBuffer = SortRecordBuffer.Allocate(size);
      var sortChunkBuffer = new SortChunkBuffer(sortRecordBuffer, infix);

      var result = new SortThresholdBuffer(sortChunkBuffer);
      return result;
    }

    /// <summary>
    /// The wrapped sort chunk buffer.
    /// </summary>
    public SortChunkBuffer SortChunkBuffer => this._sortChunkBuffer;

    /// <summary>
    /// Current fill position.
    /// </summary>
    public int FillPosition { get; set; }

    /// <summary>
    /// Returns true if there's a more space in the buffer. Otherwise it should be flushed.
    /// </summary>
    public bool CanAdd()
    {
      return this.FillPosition < this.SortChunkBuffer.SortRecordBuffer.BufferSize;
    }

    /// <summary>
    /// Adds another record in the buffer.
    /// </summary>
    internal void Add(SortRecord sr)
    {
      this.SortChunkBuffer.SortRecordBuffer.Buffer[this.FillPosition] = sr;
      this.FillPosition += 1;
    }
  }
}
