namespace BigSort.Common
{
  /// <summary>
  /// Common options for the merge sort tasks.
  /// </summary>
  internal class MergeSortOptions
  {
    /// <summary>
    /// The input file path.
    /// </summary>
    public string InFilePath { get; set; }

    /// <summary>
    /// The output file path.
    /// </summary>
    public string OutFilePath { get; set; }

    /// <summary>
    /// The path for temp directory.
    /// </summary>
    public string TempDirectoryPath { get; set; }
  }
}
