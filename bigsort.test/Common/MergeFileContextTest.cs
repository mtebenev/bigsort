using System;
using System.IO.Abstractions;
using BigSort.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BigSort.Test.Common
{
  public class MergeFileContextTest
  {
    [Fact]
    public void Should_Initialize_Paths()
    {
      var mockFs = Substitute.For<IFileSystem>();
      var mockLf = Substitute.For<ILoggerFactory>();
      mockFs.File.Exists("input-file-path").Returns(true);

      var options = new FileContextOptions
      {
        InFilePath = "input-file-path",
        OutFilePath = "out-file-path",
        UseOutFile = true
      };

      var context = new FileContext(mockFs, mockLf, options);

      // Verify
      Assert.Equal("input-file-path", context.InFilePath);
      Assert.Equal("out-file-path", context.OutFilePath);
    }

    [Fact]
    public void Should_Throw_If_Input_File_Not_Exist()
    {
      var mockFs = Substitute.For<IFileSystem>();
      var mockLf = Substitute.For<ILoggerFactory>();
      Assert.Throws<InvalidOperationException>(() =>
      {
        var options = new FileContextOptions
        {
          InFilePath = "input-file-path",
          OutFilePath = "out-file-path"
        };
        new FileContext(mockFs, mockLf, options);
      });
    }

    [Fact]
    public void Should_Use_Default_Out_File()
    {
      var mockFs = Substitute.For<IFileSystem>();
      var mockLf = Substitute.For<ILoggerFactory>();
      mockFs.File.Exists(@"x:\input.txt").Returns(true);
      mockFs.Path.ChangeExtension(@"x:\input.txt", ".out").Returns("path-with-changed-ext");

      var options = new FileContextOptions
      {
        InFilePath = @"x:\input.txt",
        UseOutFile = true
      };
      var context = new FileContext(mockFs, mockLf, options);

      // Verify
      Assert.Equal(@"x:\input.txt", context.InFilePath);
      Assert.Equal(@"path-with-changed-ext", context.OutFilePath);
    }

    [Fact]
    public void Should_Remove_Out_File_If_Exists()
    {
      var mockFs = Substitute.For<IFileSystem>();
      var mockLf = Substitute.For<ILoggerFactory>();
      mockFs.File.Exists("input-file-path").Returns(true);
      mockFs.File.Exists("out-file-path").Returns(true);

      var options = new FileContextOptions
      {
        InFilePath = "input-file-path",
        OutFilePath = "out-file-path",
        UseOutFile = true
      };
      
      new FileContext(mockFs, mockLf, options);

      // Verify
      mockFs.File.Received().Delete("out-file-path");
    }

    [Fact]
    public void Should_Use_Default_Temp_Directory()
    {
      var mockFs = Substitute.For<IFileSystem>();
      var mockLf = Substitute.For<ILoggerFactory>();
      mockFs.File.Exists("input-file-path").Returns(true);
      mockFs.Path.GetTempFileName().Returns(
        "temp-file-1",
        "temp-file-2"
      );

      var options = new FileContextOptions
      {
        InFilePath = "input-file-path",
      };
      var context = new FileContext(mockFs, mockLf, options);
      var tempFile1 = context.AddTempFile();
      var tempFile2 = context.AddTempFile();

      // Verify
      Assert.Equal("temp-file-1", tempFile1);
      Assert.Equal("temp-file-2", tempFile2);
    }

    [Fact]
    public void Should_Use_Custom_Temp_Directory()
    {
      var mockFs = Substitute.For<IFileSystem>();
      var mockLf = Substitute.For<ILoggerFactory>();
      mockFs.File.Exists("input-file-path").Returns(true);
      mockFs.Path.GetRandomFileName().Returns(
        "temp-file-1",
        "temp-file-2"
      );
      mockFs.Path.Combine(@"x:\", "temp-file-1").Returns("result-temp-1");
      mockFs.Path.Combine(@"x:\", "temp-file-2").Returns("result-temp-2");

      var options = new FileContextOptions
      {
        InFilePath = "input-file-path",
        TempDirectoryPath = @"x:\"
      };
      var context = new FileContext(mockFs, mockLf, options);
      var tempFile1 = context.AddTempFile();
      var tempFile2 = context.AddTempFile();

      // Verify
      Assert.Equal("result-temp-1", tempFile1);
      Assert.Equal("result-temp-2", tempFile2);
    }

    [Fact]
    public void Should_Remove_Temp_Files_On_Dispose()
    {
      var mockFs = Substitute.For<IFileSystem>();
      var mockLf = Substitute.For<ILoggerFactory>();
      mockFs.File.Exists("input-file-path").Returns(true);
      mockFs.Path.GetTempFileName().Returns(
        "temp-file-1",
        "temp-file-2"
      );

      var options = new FileContextOptions
      {
        InFilePath = "input-file-path",
      };
      var context = new FileContext(mockFs, mockLf, options);
      context.AddTempFile();
      context.AddTempFile();
      context.Dispose();

      // Verify
      mockFs.File.Received().Delete("temp-file-1");
      mockFs.File.Received().Delete("temp-file-2");
    }
  }
}
