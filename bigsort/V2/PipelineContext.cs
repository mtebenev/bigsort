using System;
using System.Collections.Generic;
using System.Linq;

namespace BigSort.V2
{
  internal class PipelineContext : IPipelineContext
  {
    private bool _isReadingCompleted;
    private List<long> _completedBuckets;

    public PipelineContext()
    {
      this._isReadingCompleted = false;
      this._completedBuckets = new List<long>();
    }

    public bool IsReadingCompleted => this._isReadingCompleted;

    public bool IsBucketFlushed(long infix)
    {
      var result = this._completedBuckets.Any(i => i == infix);
      return result;
    }

    public void SetBucketFlushed(long infix)
    {
      Console.WriteLine($"Context: bucket set flushed: {infix}");
      if(this._completedBuckets.Any(i => i == infix))
      {
        throw new NotImplementedException();
      }
      this._completedBuckets.Add(infix);
    }

    public void SetReadingCompleted()
    {
      this._isReadingCompleted = true;
    }
  }
}
