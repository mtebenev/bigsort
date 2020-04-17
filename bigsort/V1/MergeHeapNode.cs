namespace BigSort.V1
{
  /// <summary>
  /// Keeps the necessary information during the merge process.
  /// </summary>
  internal class MergeHeapNode
  {
    public MergeHeapNode(int fileIndex, string data)
    {
      this.FileIndex = fileIndex;
      this.Data = data;
    }

    /// <summary>
    /// The file index.
    /// </summary>
    public int FileIndex { get; }

    /// <summary>
    /// The current record.
    /// </summary>
    public string Data { get; }
  }
}
