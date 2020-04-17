using System.Threading.Tasks;
using BigSort.Common;

namespace BigSort.V2
{
  /// <summary>
  /// The V2 implementation.
  /// </summary>
  internal class MergeSortTaskV2 : IMergeSortTask
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
