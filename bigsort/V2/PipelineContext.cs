using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// Ctor.
    /// </summary>
    public PipelineContext()
    {
      this._completedBuckets = new List<long>();
    }

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
  }
}
