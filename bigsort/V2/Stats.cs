using System;

namespace BigSort.V2
{
  /// <summary>
  /// Encapsulates statistics about the sort-merge process.
  /// </summary>
  internal class Stats
  {
    private long _blockReads;
    private long _chunkFlushes;
    private long _bucketkMerges;

    public Stats()
    {
      this._blockReads = 0;
      this._chunkFlushes = 0;
      this._bucketkMerges = 0;
    }

    /// <summary>
    /// Prints the statistics to console.
    /// </summary>
    public void PrintStats()
    {
      Console.WriteLine("Statistics:");
      Console.WriteLine($"Block reads: {this._blockReads}");
      Console.WriteLine($"Chunk flushes: {this._chunkFlushes}");
      Console.WriteLine($"Bucket merges: {this._bucketkMerges}");
    }

    // Stat counters
    public void AddBlockReads()
    {
      this._blockReads++;
    }

    public void AddChunkFlushes()
    {
      this._chunkFlushes++;
    }


    public void AddBucketMerges()
    {
      this._bucketkMerges++;
    }
  }
}
