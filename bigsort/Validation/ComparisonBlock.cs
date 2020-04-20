using System;
using System.Threading.Tasks.Dataflow;
using BigSort.V2.Events;

namespace BigSort.Validation
{
  /// <summary>
  /// Validates data blocks in a file with db, record by record.
  /// </summary>
  internal class ComparisonBlock
  {
    public static ITargetBlock<Tuple<DataRecord[], BufferReadEvent>> Create()
    {
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
          var dbNumStr = t.Item1[i].DataNum.ToString();

          // Number comparison
          var numEquality = string.CompareOrdinal(
            dbNumStr,
            0,
            t.Item2.Buffer.Buffer[i],
            0,
            Math.Max(dbNumStr.Length, dotPos)
          );

          // String comparing
          var strEquality = string.CompareOrdinal(
            t.Item1[i].DataStr,
            0,
            t.Item2.Buffer.Buffer[i],
            dotPos + 1,
            int.MaxValue
          );

          if(numEquality != 0 || strEquality != 0)
          {
            var errMessage = $@"Validation failed:
line: xxx
expected: {t.Item1[i].DataNum}. {t.Item1[i].DataStr}
actual: {t.Item2.Buffer.Buffer[i]}
";
            throw new InvalidOperationException(errMessage);
          }
        }
      });

      return block;
    }
  }
}
