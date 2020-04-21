namespace BigSort.Common
{
  /// <summary>
  /// Utility class for tracking file operation progress (diagnostics).
  /// Assumes that the file is plain-text with 1-byte symbols and 2-byte line endings.
  /// </summary>
  internal class FileProgressCounter
  {
    private readonly long _fileSize;
    private long _lineCounter;
    private long _symbolCounter;

    /// <summary>
    /// Ctor.
    /// </summary>
    public FileProgressCounter(long fileSize)
    {
      this._fileSize = fileSize;
      this._lineCounter = 0;
      this._symbolCounter = 0;
    }

    /// <summary>
    /// The current line.
    /// </summary>
    public long CurrentLine => this._lineCounter;

    /// <summary>
    /// Call to update stats.
    /// </summary>
    public void OnLineProcessed(string line)
    {
      this._lineCounter++;
      this._symbolCounter += line.Length + 2;
    }

    /// <summary>
    /// Calculates progress percentage and formats 
    /// </summary>
    public string GetProgressText()
    {
      var percentage = (double)this._symbolCounter / this._fileSize;
      var result = $"lines: {this._lineCounter}, ~{percentage:P}";
      return result;
    }
  }
}
