using System;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2;
using BigSort.V2.Events;

namespace BigSort.Validation
{
  /// <summary>
  /// Performs simple record order check.
  /// </summary>
  internal static class CheckOrderBlock
  {
    public static ITargetBlock<BufferReadEvent> Create(long fileSize)
    {
      var progressCounter = new FileProgressCounter(fileSize);
      var comparer = new SortRecordComparer();
      string prevLine = null;
      var block = new ActionBlock<BufferReadEvent>(evt =>
      {
        int start = 0;
        if(prevLine == null)
        {
          prevLine = evt.Buffer.Buffer[0];
          start = 1;
          progressCounter.OnLineProcessed(evt.Buffer.Buffer[0]);
        }

        for(int i = start; i < evt.Buffer.BufferSize; i++)
        {
          if(i > 0 && i % 100000 == 0)
          {
            Console.WriteLine($"Progress: {progressCounter.GetProgressText()}");
          }

          var r1 = new SortRecord(prevLine);
          var r2 = new SortRecord(evt.Buffer.Buffer[i]);

          if(comparer.Compare(r1, r2) > 0)
          {
            throw new InvalidOperationException($"Invalid data at line: {progressCounter.CurrentLine}\n{r2.Value}");
          }

          progressCounter.OnLineProcessed(evt.Buffer.Buffer[i]);
          prevLine = evt.Buffer.Buffer[i];
        }
      });

      return block;
    }
  }
}
