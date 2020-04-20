using System;
using System.Text;
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
      var infixBytes = Encoding.Unicode.GetBytes("abcd");
      var expectedInfix = BitConverter.ToInt64(infixBytes);

      Assert.Equal(expectedInfix, r.Infix);
    }
  }
}
