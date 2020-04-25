using BigSort.V2;
using Xunit;

namespace BigSort.Test.V2
{
  public class SortRecordTest
  {
    [Fact]
    public void Create_Record()
    {
      var r = new SortRecord("123. aaa bbb ccc");
      Assert.Equal("123. aaa bbb ccc", r.Value);
    }

    [Fact]
    public void Create_Infix()
    {
      var r = new SortRecord("123. abcd");
      var expectedInfix = InfixUtils.StringToInfix("abcd");

      Assert.Equal(expectedInfix, r.Infix);
    }

    /// <summary>
    /// A record can be "1. a" - the effective infix is one symbol long
    /// </summary>
    [Fact]
    public void Should_Work_With_Short_Infixes()
    {
      var r1 = new SortRecord("123. a");
      var r2 = new SortRecord("123. aa");
      var r3 = new SortRecord("123. aaa");

      Assert.Equal(InfixUtils.StringToInfix("a\0\0\0"), r1.Infix);
      Assert.Equal(InfixUtils.StringToInfix("aa\0\0"), r2.Infix);
      Assert.Equal(InfixUtils.StringToInfix("aaa\0"), r3.Infix);
    }
  }
}
