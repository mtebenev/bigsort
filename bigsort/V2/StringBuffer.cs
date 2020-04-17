namespace BigSort.V2
{
  /// <summary>
  /// Keeps strings for processing. Really used number of strings is BufferSize.
  /// We pre-allocate the string buffers.
  /// </summary>
  public class StringBuffer
  {
    public StringBuffer(string[] buffer, int bufferSize)
    {
      this.Buffer = buffer;
      this.BufferSize = bufferSize;
    }

    public string[] Buffer { get; }
    public int BufferSize { get; }
  }
}
