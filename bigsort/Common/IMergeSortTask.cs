using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BigSort.Common
{
  /// <summary>
  /// Common interface for merge sort execution.
  /// </summary>
  internal interface IMergeSortTask
  {
    /// <summary>
    /// Runs the merge sort.
    /// </summary>
    Task ExecuteAsync(IFileContext fileContext, ILoggerFactory loggerFactory, MergeSortOptions options);
  }
}
