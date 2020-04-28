using System;
using System.Linq;
using System.Text;
using BigSort.Common;
using Xunit;

namespace BigSort.Test.Common
{
  public class BufferStringEnumeratorTest
  {
    [Fact]
    public void Enumerate_Strings()
    {
      var originalString = "aaa\r\nbbb\r\nccc";
      var buffer = Encoding.ASCII.GetBytes(originalString);

      var result = BufferStringEnumerator
        .EnumerateStrings(buffer.AsMemory())
        .ToArray();

      var expected = new[]
      {
        "aaa",
        "bbb",
        "ccc"
      };

      Assert.Equal(expected, result);
    }

    [Fact]
    public void Ignore_Empty_Lines()
    {
      var originalString = "\r\n\r\naaa\r\n\r\nbbb\r\nccc\r\n\r\n";
      var buffer = Encoding.ASCII.GetBytes(originalString);

      var result = BufferStringEnumerator
        .EnumerateStrings(buffer.AsMemory())
        .ToArray();

      var expected = new[]
      {
        "aaa",
        "bbb",
        "ccc"
      };

      Assert.Equal(expected, result);
    }

    [Fact]
    public void Process_Empty_Buffer()
    {
      var originalString = "";
      var buffer = Encoding.ASCII.GetBytes(originalString);

      var result = BufferStringEnumerator
        .EnumerateStrings(buffer.AsMemory())
        .ToArray();

      var expected = new string[0];

      Assert.Equal(expected, result);
    }

    [Fact]
    public void Process_Empty_Buffer_2()
    {
      var originalString = "\r\n\r\n\r\n";
      var buffer = Encoding.ASCII.GetBytes(originalString);

      var result = BufferStringEnumerator
        .EnumerateStrings(buffer.AsMemory())
        .ToArray();

      var expected = new string[0];

      Assert.Equal(expected, result);
    }
  }
}
