using System;
using System.Collections.Generic;
using System.Text;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates lines from a fixed dictionary.
  /// Note: not thread-safe.
  /// </summary>
  internal class LineGeneratorDictionary : ILineGenerator
  {
    private readonly Random _random;
    private readonly StringBuilder _stringBuilder;
    private readonly string[] _allWords;

    /// <summary>
    /// Ctor.
    /// </summary>
    public LineGeneratorDictionary()
    {
      this._random = new Random();
      this._stringBuilder = new StringBuilder();
      this._allWords = new[] { "Apple", "Banana", "Canon", "Dominant", "Ellipse", "Frozen", "Gilbert", "Hannover" };
    }

    /// <summary>
    /// ILineGenerator.
    /// </summary>
    public IEnumerable<string> GenerateLines()
    {
      while(true)
      {
        this._stringBuilder.Clear();
        this._stringBuilder.Append(this._random.Next());
        this._stringBuilder.Append(". ");
        this._stringBuilder.Append(this._allWords[this._random.Next(this._allWords.Length - 1)]);

        yield return this._stringBuilder.ToString();
      }
    }
  }
}
