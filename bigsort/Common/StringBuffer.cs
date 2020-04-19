namespace BigSort.Common
{
  /// <summary>
  /// Keeps strings for processing. Really used number of strings is BufferSize.
  /// We pre-allocate the string buffers.
  /// </summary>
  public class StringBuffer
  {
    public StringBuffer(string[] buffer, int bufferSize, bool isReadingCompleted)
    {
      this.Buffer = buffer;
      this.BufferSize = bufferSize;
      this.IsReadingCompleted = isReadingCompleted;
    }

    public string[] Buffer { get; }
    public int BufferSize { get; }
    public bool IsReadingCompleted { get; }
  }
}
