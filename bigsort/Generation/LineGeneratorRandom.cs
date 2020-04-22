using System;
using System.Collections.Generic;
using System.Text;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates random strings.
  /// Note: NOT thread-safe.
  /// </summary>
  internal class LineGeneratorRandom : ILineGenerator
  {
    private readonly Random _random;
    private readonly StringBuilder _stringBuilder;
    private const string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly int _alphabeSize = _chars.Length;

    /// <summary>
    /// Ctor.
    /// </summary>
    public LineGeneratorRandom()
    {
      this._random = new Random();
      this._stringBuilder = new StringBuilder();
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
        this._stringBuilder.Append(this.GenerateRandomString(10));

        yield return this._stringBuilder.ToString();
      }
    }

    /// <summary>
    /// Generates array of strings.
    /// </summary>
    private string GenerateRandomString(int size)
    {
      var result = string.Create(size, (s: size, alps: this._alphabeSize, chars: _chars, r: this._random), (buffer, state) =>
      {
        for(int i = 0; i < state.s ; i++)
        {
          buffer[i] = state.chars[state.r.Next(state.alps)];
        }
      });

      return result;
    }
  }
}
