using BigSort.Common;

namespace BigSort.V2.Events
{
  /// <summary>
  /// Sent when a block of strings has been read from the source file.
  /// </summary>
  internal class BufferReadEvent
  {
    /// <summary>
    /// Ctor.
    /// </summary>
    public BufferReadEvent(StringBuffer buffer, bool isReadCompleted)
    {
      this.Buffer = buffer;
      this.IsReadCompleted = isReadCompleted;
    }

    /// <summary>
    /// The source buffer.
    /// </summary>
    public StringBuffer Buffer { get; }

    /// <summary>
    /// True when the block is final. This is the hint that the chunk merge could start.
    /// </summary>
    public bool IsReadCompleted { get; }
  }
}
