using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;
using BigSort.V2.Events;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using Microsoft.Extensions.Logging;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// Performs the final merge.
  /// </summary>
  internal class FinalMergeBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static ITargetBlock<BucketMergeEvent[]> Create(IPipelineContext pipelineContext)
    {
      var logger = pipelineContext.LoggerFactory.CreateLogger(nameof(FinalMergeBlock));
      var result = new ActionBlock<BucketMergeEvent[]>(events =>
      {
        var sb = new StringBuilder();
        sb.AppendLine($"FinalMergeBlock.Execute(). Files: ");
        for(var i = 0; i < events.Length; i++)
        {
          sb.AppendLine($"File {i}: {events[i].FilePath}");
        }
        logger.LogDebug(sb.ToString());

        var filePaths = events
          .Select(e => e.FilePath)
          .ToList();

        using(Markers.EnterSpan("Final merge"))
        {
          FinalMerger.Merge(filePaths, pipelineContext.FileContext.OutFilePath);
        }
      });

      return result;
    }
  }
}
