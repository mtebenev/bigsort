namespace BigSort.Common
{
  /// <summary>
  /// Common options for the merge sort tasks.
  /// </summary>
  internal class MergeSortOptions
  {
    /// <summary>
    /// Max parallel jobs for CPU-intensive tasks.
    /// </summary>
    public int MaxConcurrentJobs { get; set; }
  }
}
