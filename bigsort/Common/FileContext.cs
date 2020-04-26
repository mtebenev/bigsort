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
    private readonly bool _isRemoveTempDirectory;

    /// <summary>
    /// Ctor.
    /// </summary>
    public FileContext(IFileSystem fileSystem, ILoggerFactory loggerFactory, FileContextOptions options)
    {
      this._fileSystem = fileSystem;
      this._logger = loggerFactory.CreateLogger(nameof(FileContext));
      this._tempPaths = new ConcurrentBag<string>();
      this.InFilePath = options.InFilePath;

      if(string.IsNullOrEmpty(options.TempDirectoryPath))
      {
        var tempDirectory = this._fileSystem.Path.GetTempPath();
        this._tempDirectoryPath = this._fileSystem.Path.Combine(
          tempDirectory,
          this._fileSystem.Path.GetRandomFileName());
        this._isRemoveTempDirectory = true;
      }
      else
      {
        this._tempDirectoryPath = options.TempDirectoryPath;
        this._isRemoveTempDirectory = false; // Do not remove the temporary directory if set explicitly.
      }

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
    public IFileSystem FileSystem => this._fileSystem;

    /// <summary>
    /// IFileContext.
    /// </summary>
    public string InFilePath { get; }

    /// <summary>
    /// IFileContext.
    /// </summary>
    public long GetInFileSize()
    {
      return this._fileSystem.FileInfo.FromFileName(this.InFilePath).Length;
    }

    /// <summary>
    /// IFileContext.
    /// </summary>
    public string OutFilePath { get; }

    /// <summary>
    /// IFileContext.
    /// </summary>
    public string AddTempFile(string prefix)
    {
      var tempFileName = string.IsNullOrEmpty(prefix)
        ? this._fileSystem.Path.GetRandomFileName()
        : $"{prefix}-{this._fileSystem.Path.GetRandomFileName()}";
      var newTempPath = this._fileSystem.Path.Combine(
        this._tempDirectoryPath,
        tempFileName
      );

      this._tempPaths.Add(newTempPath);

      // Lazy verify for the custom temp dir.
      if(!string.IsNullOrEmpty(this._tempDirectoryPath) && !this._fileSystem.Directory.Exists(this._tempDirectoryPath))
      {
        this._fileSystem.Directory.CreateDirectory(this._tempDirectoryPath);
      }

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
      if(this._isRemoveTempDirectory && this._tempPaths.Count > 0)
      {
        this.RemoveTempDirectory();
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

    private void RemoveTempDirectory()
    {
      try
      {
        this._fileSystem.Directory.Delete(this._tempDirectoryPath);
      }
      catch(Exception e)
      {
        this._logger.LogError(e, $"Could not delete the temporary directory: {this._tempDirectoryPath}");
      }
    }
  }
}
