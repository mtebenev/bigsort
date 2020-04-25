using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BigSort.Common;
using Microsoft.Extensions.Logging;

namespace BigSort.V2
{
  /// <summary>
  /// The pipeline context implementation.
  /// </summary>
  internal class PipelineContext : IPipelineContext
  {
    private readonly TaskCompletionSource<uint[]> _tcsInfixesReady;
    private readonly HashSet<uint> _infixes; // All discovered infixes.
    private readonly ILogger _logger;
    private ConcurrentDictionary<uint, int> _chunkFlushesMap;
    private ConcurrentDictionary<uint, int> _chunkStartsMap;

    /// <summary>
    /// Ctor.
    /// </summary>
    public PipelineContext(ILoggerFactory loggerFactory, IFileContext fileContext)
    {
      this.LoggerFactory = loggerFactory;
      this.FileContext = fileContext;
      this.Stats = new Stats();
      this._logger = loggerFactory.CreateLogger(nameof(PipelineContext));
      this._tcsInfixesReady = new TaskCompletionSource<uint[]>();
      this._infixes = new HashSet<uint>();
      this._chunkFlushesMap = new ConcurrentDictionary<uint, int>();
      this._chunkStartsMap = new ConcurrentDictionary<uint, int>();
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public IFileContext FileContext { get; }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public Stats Stats { get; }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public Task<uint[]> GetAllInfixesAsync()
    {
      return this._tcsInfixesReady.Task;
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void AddInfix(uint infix)
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

    public void OnChunkFlush(uint infix)
    {
      this._chunkFlushesMap.AddOrUpdate(infix, 1, (k, v) => v + 1);
    }

    public void OnChunkStart(uint infix)
    {
      this._chunkStartsMap.AddOrUpdate(infix, 1, (k, v) => v + 1);
    }

    public bool IsBucketFullyFlushed(uint infix)
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
