namespace BigSort.Common
{
  /// <summary>
  /// Defines a string location in the memory buffer.
  /// All positions and the length are in bytes.
  /// </summary>
  internal readonly struct BufferStringPointer
  {
    public BufferStringPointer(int start, int length, int dotPos)
    {
      this.Start = start;
      this.Length = length;
      this.DotPos = dotPos;
    }

    /// <summary>
    /// Start position.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// The string length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// The dot position.
    /// </summary>
    public int DotPos { get; }
  }
}
