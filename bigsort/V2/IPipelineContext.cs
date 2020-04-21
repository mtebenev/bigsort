using System.Threading.Tasks;
using BigSort.Common;
using Microsoft.Extensions.Logging;

namespace BigSort.V2
{
  /// <summary>
  /// Contextual information during the pipeline executionl.
  /// </summary>
  internal interface IPipelineContext
  {
    /// <summary>
    /// The logger factory instance.
    /// </summary>
    ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// The file context.
    /// </summary>
    IFileContext FileContext { get; }

    /// <summary>
    /// The pipeline statistics.
    /// </summary>
    Stats Stats { get; }

    /// <summary>
    /// Resolved as soon as all possible infixes are known.
    /// That is when the source buffer block has processed all incoming strings.
    /// </summary>
    Task<long[]> GetAllInfixesAsync();

    /// <summary>
    /// All discovered infixes should be added with this method.
    /// To track sort/merge process and start merging earlier.
    /// </summary>
    void AddInfix(long infix);

    /// <summary>
    /// The pipeline should indicate when all infixes are discovered (no more new data from the source file).
    /// </summary>
    void OnInfixesReady();

    /// <summary>
    /// Invoke on the chunk flush for tracking start/flush pairs.
    /// </summary>
    void OnChunkStart(long infix);

    /// <summary>
    /// Invoke on the chunk flush for tracking start/flush pairs.
    /// </summary>
    void OnChunkFlush(long infix);

    /// <summary>
    /// The pipeline should check if the whole bucket has been flushed.
    ///  After that the bucket's chunks could be merged.
    /// </summary>
    bool IsBucketFullyFlushed(long infix);
  }
}
