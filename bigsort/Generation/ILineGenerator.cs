using System;
using System.Collections.Generic;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates test data lines.
  /// </summary>
  internal interface ILineGenerator
  {
    /// <summary>
    /// Fills the memory buffer with generated data.
    /// </summary>
    int FillBuffer(Span<char> memBuffer, long toFill);
  }
}
