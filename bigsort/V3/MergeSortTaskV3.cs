using System.Threading.Tasks;
using BigSort.Common;

namespace BigSort.V3
{
  /// <summary>
  /// The V3 implementation.
  /// </summary>
  internal class MergeSortTaskV3 : IMergeSortTask
  {
    /// <summary>
    /// IMergeSortTask.
    /// </summary>
    public async Task ExecuteAsync(MergeSortOptions options)
    {
      var reader = new SourceReader();
      var (startBlock, finishBlock) = PipelineBuilder.Build(options);

      reader.Start(options.InFilePath, startBlock);

      await finishBlock.Completion;
    }
  }
}
