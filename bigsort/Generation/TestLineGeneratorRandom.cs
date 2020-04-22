using System.Collections.Generic;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates random strings.
  /// </summary>
  internal class TestLineGeneratorRandom : ITestLineGenerator
  {
    /// <summary>
    /// ITestLineGenerator.
    /// </summary>
    public IEnumerable<string> GenerateLines()
    {
      int counter = 0;
      while(true)
      {
        yield return $"Random {counter++}";
      }
    }
  }
}
