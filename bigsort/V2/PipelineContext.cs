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
    private readonly TaskCompletionSource<ushort[]> _tcsInfixesReady;
    private readonly HashSet<ushort> _infixes; // All discovered infixes.
    private readonly ILogger _logger;
    private ConcurrentDictionary<ushort, int> _chunkFlushesMap;
    private ConcurrentDictionary<ushort, int> _chunkStartsMap;

    /// <summary>
    /// Ctor.
    /// </summary>
    public PipelineContext(ILoggerFactory loggerFactory, IFileContext fileContext)
    {
      this.LoggerFactory = loggerFactory;
      this.FileContext = fileContext;
      this.Stats = new Stats();
      this._logger = loggerFactory.CreateLogger(nameof(PipelineContext));
      this._tcsInfixesReady = new TaskCompletionSource<ushort[]>();
      this._infixes = new HashSet<ushort>();
      this._chunkFlushesMap = new ConcurrentDictionary<ushort, int>();
      this._chunkStartsMap = new ConcurrentDictionary<ushort, int>();
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
    public Task<ushort[]> GetAllInfixesAsync()
    {
      return this._tcsInfixesReady.Task;
    }

    /// <summary>
    /// IPipelineContext.
    /// </summary>
    public void AddInfix(ushort infix)
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

    public void OnChunkFlush(ushort infix)
    {
      this._chunkFlushesMap.AddOrUpdate(infix, 1, (k, v) => v + 1);
    }

    public void OnReceivedChunk(ushort infix)
    {
      this._chunkStartsMap.AddOrUpdate(infix, 1, (k, v) => v + 1);
    }

    public bool IsBucketFullyFlushed(ushort infix)
    {
      if(!this._chunkStartsMap.ContainsKey(infix) || !this._chunkFlushesMap.ContainsKey(infix))
      {
        // Probably something wrong in the pipeline. Discovered buckets lost?
        throw new InvalidOperationException("Number or flushed buckets is different from number of the found buckets.");
      }

      // Note: checking the infix reading status without awaiting. We don't want blocking callers on this call.
      var result = this._tcsInfixesReady.Task.IsCompletedSuccessfully && this._chunkStartsMap[infix] == this._chunkFlushesMap[infix];
      return result;
    }
  }
}
