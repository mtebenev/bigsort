namespace BigSort.V2.Events
{
  /// <summary>
  /// Sent when a single chunk file has been saved for a bucket.
  /// </summary>
  internal class BucketChunkFlushEvent
  {
    /// <summary>
    /// Ctor.
    /// <paramref name="isFinalChunk">Hint for merge coordinator: he can start earlier if the bucked has been completed.</paramref>
    /// </summary>
    public BucketChunkFlushEvent(string filePath, long infix, bool isFinalChunk)
    {
      this.FilePath = filePath;
      this.Infix = infix;
      this.IsFinalChunk = isFinalChunk;
    }

    /// <summary>
    /// The chunk path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The infix of the chunk.
    /// </summary>
    public long Infix { get; }
    public bool IsFinalChunk { get; }
  }
}
