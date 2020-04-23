using System;
using System.IO;
using System.Text;
using BigSort.Common;
using Xunit;

namespace BigSort.Test.Common
{
  public class StringBufferWriterTest
  {
    [Fact]
    public void Write_Bufer()
    {
      var sb = new StringBuilder();
      var sw = new StringWriter(sb);
      var source = "ABC\nDEF\nGHI\n";

      StringBufferWriter.WriteBuffer(source.AsSpan(), sw);

      Assert.Equal("ABC\r\nDEF\r\nGHI\r\n", sb.ToString());
    }
  }
}
