using System.Threading.Tasks.Dataflow;
using BigSort.V3.Events;

namespace BigSort.V3.Blocks
{
  /// <summary>
  /// Sorts strings in the buffer (actually sort the pointers).
  /// </summary>
  internal static class BufferSortBlock
  {
    public static ITargetBlock<BufferReadEvent3> Create()
    {
      var result = new ActionBlock<BufferReadEvent3>(buffer =>
      {
        buffer.Dispose();
      });

      return result;
    }
  }
}
