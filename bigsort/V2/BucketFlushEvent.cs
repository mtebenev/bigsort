namespace BigSort.V2
{
  /// <summary>
  /// Sent when a chunk file has been saved for a bucket.
  /// </summary>
  internal class BucketFlushEvent
  {
    /// <summary>
    /// Ctor.
    /// </summary>
    public BucketFlushEvent(string filePath, long infix)
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
    public long Infix { get; }
  }
}
