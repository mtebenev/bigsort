using System;
using System.Collections.Concurrent;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace BigSort.Common
{
  /// <summary>
  /// Manages files related to the commands.
  /// </summary>
  internal class FileContext : IFileContext, IDisposable
  {
    private readonly string _tempDirectoryPath;
    private readonly ConcurrentBag<string> _tempPaths;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    /// <summary>
    /// Ctor.
    /// </summary>
    public FileContext(IFileSystem fileSystem, ILoggerFactory loggerFactory, FileContextOptions options)
    {
      this._fileSystem = fileSystem;
      this._logger = loggerFactory.CreateLogger(nameof(FileContext));
      this._tempPaths = new ConcurrentBag<string>();
      this._tempDirectoryPath = options.TempDirectoryPath;
      this.InFilePath = options.InFilePath;

      if(options.UseOutFile)
      {
        this.OutFilePath = !string.IsNullOrEmpty(options.OutFilePath)
          ? options.OutFilePath
          : fileSystem.Path.ChangeExtension(options.InFilePath, ".out");
      }

      // Check if input exists
      if(!fileSystem.File.Exists(this.InFilePath))
      {
        throw new InvalidOperationException($"Could not find the input file: {this.InFilePath}");
      }

      if(options.UseOutFile && fileSystem.File.Exists(this.OutFilePath))
      {
        fileSystem.File.Delete(this.OutFilePath);
      }
    }

    /// <summary>
    /// IFileContext.
    /// </summary>
    public string InFilePath { get; }

    /// <summary>
    /// IFileContext.
    /// </summary>
    public string OutFilePath { get; }

    /// <summary>
    /// IFileContext.
    /// </summary>
    public string AddTempFile()
    {
      var newTempPath = string.IsNullOrEmpty(this._tempDirectoryPath)
        ? this._fileSystem.Path.GetTempFileName()
        : this._fileSystem.Path.Combine(this._tempDirectoryPath, this._fileSystem.Path.GetRandomFileName());

      this._tempPaths.Add(newTempPath);
      return newTempPath;
    }

    /// <summary>
    /// IDisposable.
    /// </summary>
    public void Dispose()
    {
      foreach(var p in this._tempPaths)
      {
        this.DeleteTempFile(p);
      }
    }

    private void DeleteTempFile(string path)
    {
      try
      {
        this._fileSystem.File.Delete(path);
      }
      catch(Exception e)
      {
        this._logger.LogError(e, $"Could not delete the temporary file: {path}");
      }
    }
  }
}
