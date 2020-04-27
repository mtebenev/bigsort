namespace BigSort.Common
{
  /// <summary>
  /// Defines a string location in the memory buffer.
  /// </summary>
  internal struct BufferStringPointer
  {
    public BufferStringPointer(int start, int length)
    {
      this.Start = start;
      this.Length = length;
    }

    /// <summary>
    /// Start position (in bytes)
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// The string length (in bytes)
    /// </summary>
    public int Length { get; set; }
  }
}
