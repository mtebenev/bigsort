using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using MoreLinq;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates necessary amount of the data.
  /// </summary>
  internal static class TestDataGenerator
  {
    /// <summary>
    /// Generates the data.
    /// </summary>
    public static void Start(long toGenerateTotal, ITestLineGenerator lineGenerator, ITargetBlock<StringBuffer> target)
    {
      var bufferSize = 1000;
      long symbolsGenerated = 0;

      while(symbolsGenerated < toGenerateTotal)
      {
        var (buffer, generatedBlockSize) = TestDataGenerator.GenerateBuffer(lineGenerator, bufferSize);
        target.Post(buffer);
        symbolsGenerated += generatedBlockSize;
      }
      target.Complete();
    }

    /// <summary>
    /// Generates lines and returns the string buffer & size of the generated block.
    /// Note: the result size could be slightly bigger than required, but it's not important for this task.
    /// </summary>
    private static (StringBuffer, long) GenerateBuffer(ITestLineGenerator lineGenerator, long toGenerate)
    {
      // Take the next line, add its length + 2 for line end
      // Repeat until we reach the required buffer size.
      var finalLength = 0;

      var lines = lineGenerator
        .GenerateLines()
        .TakeUntil(s =>
        {
          finalLength += s.Length + 2;
          return finalLength > toGenerate;
        })
        .ToArray();

      var stringBuffer = new StringBuffer(lines, lines.Length);
      return (stringBuffer, finalLength);
    }
  }
}
