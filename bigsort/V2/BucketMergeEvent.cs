namespace BigSort.V2
{
  /// <summary>
  /// Sent when a bucket has been fully merged.
  /// </summary>
  internal class BucketMergeEvent
  {
    public BucketMergeEvent(string filePath, long infix)
    {
      this.FilePath = filePath;
      this.Infix = infix;
    }

    /// <summary>
    /// The result file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The bucket infix.
    /// </summary>
    public long Infix { get; }
  }
}
