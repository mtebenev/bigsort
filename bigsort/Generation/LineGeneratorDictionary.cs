using System;
using System.Linq;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates lines from a fixed dictionary.
  /// Note: not thread-safe.
  /// </summary>
  internal class LineGeneratorDictionary : ILineGenerator
  {
    private readonly Random _random;
    private readonly string[] _allWords;
    private readonly int _maxStringLength;
    
    /// <summary>
    /// Ctor.
    /// </summary>
    public LineGeneratorDictionary(string[] dictionary)
    {
      this._random = new Random();
      this._allWords = (string[])dictionary.Clone();
      this._maxStringLength = this._allWords.Max(s => s.Length) + 20 + 2 + 1; // Max string length is the longest word + long for number + dot + space + line end
    }

    /// <summary>
    /// ILineGenerator.
    /// </summary>
    public int FillBuffer(Span<char> memBuffer, long toFill)
    {
      var workSpan = memBuffer;
      while(workSpan.Length > this._maxStringLength)
      {
        // The number
        var rn = this._random.Next();
        rn.TryFormat(workSpan, out var written);
        workSpan = workSpan.Slice(written);

        // Dot
        workSpan[0] = '.';
        workSpan[1] = ' ';
        workSpan = workSpan.Slice(2);

        // The string
        rn = this._random.Next(this._allWords.Length);
        this._allWords[rn].AsSpan().CopyTo(workSpan);
        workSpan = workSpan.Slice(this._allWords[rn].Length);

        // the line ending
        workSpan[0] = '\n';
        workSpan = workSpan.Slice(1);
      }

      return memBuffer.Length - workSpan.Length;
    }
  }
}
