namespace BigSort.V2.Events
{
  /// <summary>
  /// Sent when a single chunk file has been saved for a bucket.
  /// </summary>
  internal class ChunkFlushEvent
  {
    /// <summary>
    /// Ctor.
    /// </summary>
    public ChunkFlushEvent(string filePath, uint infix)
    {
      this.FilePath = filePath;
      this.Infix = infix;
    }

    /// <summary>
    /// The chunk path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The infix of the chunk.
    /// </summary>
    public uint Infix { get; }
  }
}
