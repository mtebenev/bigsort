using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BigSort.V2
{
  /// <summary>
  /// The pipeline context implementation.
  /// </summary>
  internal class PipelineContext : IPipelineContext
  {
    private List<long> _completedBuckets;
    private long _blockReads;
    private long _chunkFlushes;
    private long _bucketkMerges;
    private readonly TaskCompletionSource<long[]> _tcsInfixesReady;
    private readonly HashSet<long> _infixes; // All discovered infixes.
    private readonly ILogger _logger;

    /// <summary>
    /// Ctor.
    /// </summary>
    public PipelineContext(ILoggerFactory loggerFactory)
    {
      this.LoggerFactory = loggerFactory;
      this._logger = loggerFactory.CreateLogger(nameof(PipelineContext));
      this._completedBuckets = new List<long>();
      this._tcsInfixesReady = new TaskCompletionSource<long[]>();
      this._infixes = new HashSet<long>();
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void AddBlockReads()
    {
      this._blockReads++;
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void AddChunkFlushes()
    {
      this._chunkFlushes++;
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void AddBucketMerges()
    {
      this._bucketkMerges++;
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public bool IsBucketFlushed(long infix)
    {
      var result = this._completedBuckets.Any(i => i == infix);
      return result;
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void PrintStats()
    {
      Console.WriteLine("Statistics:");
      Console.WriteLine($"Block reads: {this._blockReads}");
      Console.WriteLine($"Chunk flushes: {this._chunkFlushes}");
      Console.WriteLine($"Bucket merges: {this._bucketkMerges}");
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void SetBucketFlushed(long infix)
    {
      if(this._completedBuckets.Any(i => i == infix))
      {
        throw new NotImplementedException();
      }
      this._completedBuckets.Add(infix);
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public Task<long[]> GetAllInfixesAsync()
    {
      return this._tcsInfixesReady.Task;
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void AddInfix(long infix)
    {
      this._infixes.Add(infix);
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void OnInfixesReady()
    {
      this._logger.LogDebug("Infixes collected.");
      this._tcsInfixesReady.SetResult(this._infixes.ToArray());
    }
  }
}
