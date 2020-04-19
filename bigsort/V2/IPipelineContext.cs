namespace BigSort.V2
{
  /// <summary>
  /// Contextual information during the pipeline executionl.
  /// </summary>
  internal interface IPipelineContext
  {
    bool IsBucketFlushed(long infix);
    void SetBucketFlushed(long infix);

    // Stat counters
    void AddBlockReads();
    void AddChunkFlushes();
    void AddBucketMerges();

    /// <summary>
    /// Prints statistics to the console.
    /// </summary>
    void PrintStats();
  }
}
