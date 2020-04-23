using System.Buffers;

namespace BigSort.Common
{
  /// <summary>
  /// The memory buffer for stings (improved version).
  /// </summary>
  class StringBuffer2
  {
    public StringBuffer2(IMemoryOwner<char> buffer, int symbolCount)
    {
      this.Buffer = buffer;
      this.SymbolCount = symbolCount;
    }

    public IMemoryOwner<char> Buffer { get; }
    public int SymbolCount { get; }
  }
}
