using System.Threading.Tasks;

namespace BigSort.V2
{
  /// <summary>
  /// Contextual information during the pipeline executionl.
  /// </summary>
  internal interface IPipelineContext
  {
    bool IsBucketFlushed(long infix);
    void SetBucketFlushed(long infix);

    /// <summary>
    /// Resolved as soon as all possible infixes are known.
    /// That is when the source buffer block has processed all incoming strings.
    /// </summary>
    Task<long[]> GetAllInfixesAsync();
    void AddInfix(long infix);
    void OnInfixesReady();

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
