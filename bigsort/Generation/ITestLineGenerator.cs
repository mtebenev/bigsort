using System.Collections.Generic;

namespace BigSort.Generation
{
  /// <summary>
  /// Generates test data lines.
  /// </summary>
  internal interface ITestLineGenerator
  {
    /// <summary>
    /// Creates the line generation enumerable.
    /// </summary>
    IEnumerable<string> GenerateLines();
  }
}
