namespace BigSort.Validation
{
  /// <summary>
  /// Simple DTO for validating data using sqlite.
  /// </summary>
  internal class DataRecord
  {
    /// <summary>
    /// Numeric part of the source string.
    /// </summary>
    public int DataNum { get; set; }

    /// <summary>
    /// String part of the source string.
    /// </summary>
    public string DataStr { get; set; }
  }
}
