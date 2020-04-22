using System.Collections.Generic;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates test data lines.
  /// </summary>
  internal interface ILineGenerator
  {
    /// <summary>
    /// Creates the line generation enumerable.
    /// </summary>
    IEnumerable<string> GenerateLines();
  }
}
