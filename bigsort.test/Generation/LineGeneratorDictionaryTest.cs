using System;
using BigSort.Generation;
using Xunit;

namespace BigSort.Test.Generation
{
  public class LineGeneratorDictionaryTest
  {
    [Fact]
    public void Generate_Buffer()
    {
      var buffer = new char[100];

      var dict = new[] { "abc" };
      var generator = new LineGeneratorDictionary(dict);
      var count = generator.FillBuffer(buffer, 100);

      var s = new string(buffer, 0, count);

      var lines = s.Split('\n', StringSplitOptions.RemoveEmptyEntries);
      foreach(var l in lines)
      {
        var parts = l.Split(new[] { '.', });
        int.Parse(parts[0]); // throws
        Assert.Equal(" abc", parts[1]);
      }
    }
  }
}
