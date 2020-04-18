using System;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;

namespace BigSort.V3
{
  /// <summary>
  /// Performs partitioning for the source records.
  /// </summary>
  internal class PartitionerBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static ActionBlock<StringBuffer> Create()
    {
      var block = new PartitionerBlock();
      var result = new ActionBlock<StringBuffer>(
        (stringBuffer) => block.Execute(stringBuffer));

      return result;
    }

    private void Execute(StringBuffer stringBuffer)
    {
    }
  }
}
