using System;
using System.Linq;
using BigSort.V2;
using Xunit;

namespace BigSort.Test.V2
{
  public class InfixComparerTest
  {
    [Fact]
    public void Should_Should_Infixes()
    {
      var infixStrings = new[]
      {
        "dddd",
        "aaaa",
        "cccc",
        "bbbb"
      };
      var infixes = infixStrings.Select(s => InfixUtils.StringToInfix(s));

      var comparer = new InfixComparer();
      var sortedInfixStrings = infixes
        .OrderBy(i => i, comparer)
        .Select(i => InfixUtils.InfixToString(i))
        .ToList();

      var expectedInfixStrings = new[]
      {
        "aaaa",
        "bbbb",
        "cccc",
        "dddd",
      };

      Assert.Equal(expectedInfixStrings, sortedInfixStrings);
    }

    [Fact]
    public void Testing_Utils_Converters()
    {
      var s = "abcd";
      var infix = InfixUtils.StringToInfix(s);
      var s2 = InfixUtils.InfixToString(infix);

      Assert.Equal(s, s2);
    }
  }
}
