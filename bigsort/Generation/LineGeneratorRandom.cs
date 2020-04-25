using System;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates lines of random 1-3 words (each of 1-8 symbols).
  /// Note: NOT thread-safe.
  /// </summary>
  internal class LineGeneratorRandom : ILineGenerator
  {
    private readonly Random _random;
    private const string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly int _alphabeSize = _chars.Length;
    private readonly int _maxStringLength;

    /// <summary>
    /// Ctor.
    /// </summary>
    public LineGeneratorRandom()
    {
      this._random = new Random();
      this._maxStringLength =  24 + 2 + 20 + 2 + 1; // Max string length is the (3 * 8symbol words) + 2 spaces between words + long for number + dot + space + line end
    }

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
        var wordCount = this._random.Next(1, 4); // Note: the upper bound is exclusive
        for(int i = 0; i < wordCount; i++)
        {
          if(i > 0)
          {
            workSpan[0] = ' ';
            workSpan = workSpan.Slice(1);
          }

          var symbolCount = this._random.Next(1, 9); // Note: the upper bound is exclusive
          for(int j = 0; j < symbolCount; j++)
          {
            rn = this._random.Next(this._alphabeSize);
            workSpan[j] = LineGeneratorRandom._chars[rn];
          }
          workSpan = workSpan.Slice(symbolCount);
        }

        // the line ending
        workSpan[0] = '\n';
        workSpan = workSpan.Slice(1);
      }

      return memBuffer.Length - workSpan.Length;
    }
  }
}
