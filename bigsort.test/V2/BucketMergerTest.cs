using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using BigSort.V2;
using NSubstitute;
using Xunit;

namespace BigSort.Test.V2
{
  public class BucketMergerTest
  {
    [Fact]
    public void Should_Use_Record_Comparer()
    {
      var lines1 = new[]
      {
        "900. aaa",
        "800. bbb",
        "700. ccc"
      };

      var lines2 = new[]
      {
        "310. aaa",
        "210. bbb",
        "110. ccc"
      };

      var lines3 = new[]
      {
        "500. aaa",
        "400. bbb",
        "300. ccc"
      };

      var mockFs = Substitute.For<IFileSystem>();
      mockFs.File.ReadLines("file1").Returns(lines1);
      mockFs.File.ReadLines("file2").Returns(lines2);
      mockFs.File.ReadLines("file3").Returns(lines3);

      var resultLines = new List<string>();
      mockFs.File.WriteAllLines("out_path", Arg.Do<IEnumerable<string>>(ls =>
      {
        resultLines = ls.ToList();
      }));

      BucketMerger.MergeKWay(
        mockFs,
        new[] { "file1", "file2", "file3" },
        "out_path"
      );

      var expectedLines = new[]
      {
        "310. aaa",
        "500. aaa",
        "900. aaa",
        "210. bbb",
        "400. bbb",
        "800. bbb",
        "110. ccc",
        "300. ccc",
        "700. ccc",
      };

      Assert.Equal(expectedLines, resultLines);
    }
  }
}
