using System.Threading.Tasks;
using BigSort.Common;
using Microsoft.Extensions.Logging;

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
    public async Task ExecuteAsync(IFileContext fileContext, ILoggerFactory loggerFactory, MergeSortOptions options)
    {
      //var splitBufferSize = 1000000; // 19mb?
      var blockSize = 6000000; // 113mb
      //var splitBufferSize = 12000000; // 226mb

      var context = new PipelineContext(loggerFactory);
      var reader = new SourceReader();
      var (startBlock, finishBlock) = PipelineBuilder.Build(options, context);

      reader.Start(options.InFilePath, blockSize, context, startBlock);

      await finishBlock.Completion;
      context.Stats.PrintStats();
    }
  }
}
