namespace BigSort.Common
{
  /// <summary>
  /// Input settings for the file context.
  /// </summary>
  internal class FileContextOptions
  {
    /// <summary>
    /// Path to the input file.
    /// </summary>
    public string InFilePath { get; set; }

    /// <summary>
    /// The out file path.
    /// Depending on the task and UseFilePath option could be ignored or auto-generated.
    /// If the output file already exists it will be deleted.
    /// </summary>
    public string OutFilePath { get; set; }

    /// <summary>
    /// If false, then the context will never try use/generate/check the out file.
    /// </summary>
    public bool UseOutFile { get; set; }

    /// <summary>
    /// Path to the temp directory. System temp will be used if not defined.
    /// Note: using custom temp directory is for dev purposes only. There is no guarantee on unique file names in this case.
    /// </summary>
    public string TempDirectoryPath { get; set; }
  }
}
