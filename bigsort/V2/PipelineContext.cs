using System;
using System.Collections.Concurrent;
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
    private readonly TaskCompletionSource<long[]> _tcsInfixesReady;
    private readonly HashSet<long> _infixes; // All discovered infixes.
    private readonly ILogger _logger;
    private ConcurrentDictionary<long, int> _chunkFlushesMap;
    private ConcurrentDictionary<long, int> _chunkStartsMap;

    /// <summary>
    /// Ctor.
    /// </summary>
    public PipelineContext(ILoggerFactory loggerFactory)
    {
      this.LoggerFactory = loggerFactory;
      this.Stats = new Stats();
      this._logger = loggerFactory.CreateLogger(nameof(PipelineContext));
      this._tcsInfixesReady = new TaskCompletionSource<long[]>();
      this._infixes = new HashSet<long>();
      this._chunkFlushesMap = new ConcurrentDictionary<long, int>();
      this._chunkStartsMap = new ConcurrentDictionary<long, int>();
  }

  /// <summary>
  /// IPipelineContext.
  /// </summary>
  public ILoggerFactory LoggerFactory { get; }


    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public Stats Stats { get; }

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

    public void OnChunkFlush(long infix)
    {
      this._chunkFlushesMap.AddOrUpdate(infix, 1, (k, v) => v + 1);
    }

    public void OnChunkStart(long infix)
    {
      this._chunkStartsMap.AddOrUpdate(infix, 1, (k, v) => v + 1);
    }

    public bool IsBucketFullyFlushed(long infix)
    {
      if(!this._chunkStartsMap.ContainsKey(infix) || !this._chunkFlushesMap.ContainsKey(infix))
      {
        // Probably something wrong in the pipeline. Discovered buckets lost?
        throw new InvalidOperationException("Number or flushed buckets is different from number of the found buckets.");
      }

      var result = this._chunkStartsMap.ContainsKey(infix) == this._chunkFlushesMap.ContainsKey(infix);
      return result;
    }
  }
}
