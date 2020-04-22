using System;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;
using Microsoft.Extensions.Logging;

namespace BigSort.Validation
{
  /// <summary>
  /// Validates data blocks in a file with db, record by record.
  /// </summary>
  internal class ComparisonBlock
  {
    public static ITargetBlock<Tuple<DataRecord[], BufferReadEvent>> Create(ILoggerFactory loggerFactory, long fileSize)
    {
      var logger = loggerFactory.CreateLogger(nameof(ComparisonBlock));
      var progressCounter = new FileProgressCounter(fileSize);
      var block = new ActionBlock<Tuple<DataRecord[], BufferReadEvent>>((t) =>
      {
        if(t.Item1.Length != t.Item2.Buffer.BufferSize)
        {
          // We actually can't say much in this case. Seems like input and output files are unrelated?
          throw new InvalidOperationException("Blocks in the file and database has different size.");
        }

        for(int i = 0; i < t.Item1.Length; i++)
        {
          var dotPos = t.Item2.Buffer.Buffer[i].IndexOf(".");
          var parsedNum = int.Parse(t.Item2.Buffer.Buffer[i].AsSpan(0, dotPos));

          // Number comparison
          var numEquality = t.Item1[i].DataNum > parsedNum
          ? 1
          : t.Item1[i].DataNum == parsedNum
            ? 0
            : -1;

          // String comparing
          var strEquality = string.CompareOrdinal(
            t.Item1[i].DataStr,
            0,
            t.Item2.Buffer.Buffer[i],
            dotPos + 1,
            int.MaxValue
          );

          // Track progress
          progressCounter.OnLineProcessed(t.Item2.Buffer.Buffer[i]);

          if(numEquality != 0 || strEquality != 0)
          {
            var errMessage = $@"Validation failed:
line: {progressCounter.CurrentLine}
expected: {t.Item1[i].DataNum}.{t.Item1[i].DataStr}
actual: {t.Item2.Buffer.Buffer[i]}
";
            throw new InvalidOperationException(errMessage);
          }
        }
        logger.LogInformation($"Progress: {progressCounter.GetProgressText()}");
      });

      return block;
    }
  }
}
