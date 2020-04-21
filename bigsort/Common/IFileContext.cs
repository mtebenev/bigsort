namespace BigSort.Common
{
  /// <summary>
  /// Client interface for the file context.
  /// </summary>
  interface IFileContext
  {
    /// <summary>
    /// The input file path.
    /// </summary>
    public string InFilePath { get; }

    /// <summary>
    /// The output file path.
    /// </summary>
    public string OutFilePath { get; }

    /// <summary>
    /// Adds unique temp file to the context.
    /// Returns the temp file path. The temp file will be removed upon disposing the context.
    /// </summary>
    public string AddTempFile();
  }
}
