namespace BigSort.V2
{
  /// <summary>
  /// Contextual information during the pipeline executionl.
  /// </summary>
  internal interface IPipelineContext
  {
    bool IsReadingCompleted { get; }

    void SetReadingCompleted();
    bool IsBucketFlushed(long infix);
    void SetBucketFlushed(long infix);

  }
}
