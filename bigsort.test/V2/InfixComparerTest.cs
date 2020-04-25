using System.Linq;
using BigSort.V2;
using Xunit;

namespace BigSort.Test.V2
{
  public class InfixComparerTest
  {
    [Fact]
    public void Should_Compare_Infixes()
    {
      var infixStrings = new[]
      {
        "dd",
        "aa",
        "cc",
        "bb"
      };
      var infixes = infixStrings.Select(s => InfixUtils.StringToInfix(s));

      var comparer = new InfixComparer();
      var sortedInfixStrings = infixes
        .OrderBy(i => i, comparer)
        .Select(i => InfixUtils.InfixToString(i))
        .ToList();

      var expectedInfixStrings = new[]
      {
        "aa",
        "bb",
        "cc",
        "dd",
      };

      Assert.Equal(expectedInfixStrings, sortedInfixStrings);
    }

    [Fact]
    public void Testing_Utils_Converters()
    {
      var s = "ab";
      var infix = InfixUtils.StringToInfix(s);
      var s2 = InfixUtils.InfixToString(infix);

      Assert.Equal(s, s2);
    }
  }
}
