using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.Generation;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using Microsoft.Extensions.Logging;

namespace BigSort.Commands
{
  /// <summary>
  /// The check command.
  /// </summary>
  [Command(Name = "generate", Description = @"Generates the data file of required size and dictionary.")]
  internal class CommandGenerate
  {
    [Required]
    [Argument(0, "Output file path")]
    public string OutFilePath { get; set; }

    [Option(LongName = "limit", Description = "Limit generated file size. Exmple: 2K, 2M, 2G")]
    public string Limit { get; set; }

    public async Task OnExecuteAsync()
    {

      var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug);
      });
      var logger = loggerFactory.CreateLogger(nameof(CommandGenerate));

      try
      {
        var limitSize = string.IsNullOrEmpty(this.Limit) ? "1GB" : this.Limit;
        var toGenerate = (long)StringUtils.ParseFileSize(limitSize, 1024);
        logger.LogInformation($"Generating file, size=~{limitSize}");

        var sw = new StreamWriter(this.OutFilePath, false);
        var storeBlock = new ActionBlock<StringBuffer>(buffer =>
        {
          this.SaveBuffer(buffer, sw);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

        Func<ILineGenerator> lineGeneratorFactory = () => new TestLineGeneratorRandom();
        TestDataGenerator.Start(toGenerate, lineGeneratorFactory, storeBlock);
        await storeBlock.Completion;

        sw.Flush();
        sw.Dispose();

        logger.LogInformation("The file has been generated successfully.");
      }
      catch(Exception e)
      {
        logger.LogCritical(e, "Test data generateion has failed.");
      }
      finally
      {
      }
    }

    private void SaveBuffer(StringBuffer buffer, StreamWriter streamWriter)
    {
      using(Markers.EnterSpan("Saving data buffer"))
      {
        for(int i = 0; i < buffer.BufferSize; i++)
        {
          streamWriter.WriteLine(buffer.Buffer[i]);
        }
      }
    }
  }
}
