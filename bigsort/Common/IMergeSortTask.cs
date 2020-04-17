using System.Threading.Tasks;

namespace BigSort.Common
{
  /// <summary>
  /// Common interface for merge sort execution.
  /// </summary>
  internal interface IMergeSortTask
  {
    Task ExecuteAsync();
  }
}
