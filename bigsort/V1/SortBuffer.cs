namespace BigSort.V1
{
  /// <summary>
  /// Keeps strings for sorting along with used number of records.
  /// </summary>
  internal class SortBuffer
  {
    public SortBuffer(string[] buffer, int bufferSize)
    {
      this.Buffer = buffer;
      this.BufferSize = bufferSize;
    }

    public string[] Buffer { get; }
    public int BufferSize { get; }
  }
}
