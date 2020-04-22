using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Threading.Tasks;
using BigSort.Common;
using BigSort.V2;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace BigSort.Commands
{
  /// <summary>
  /// The command executes merge sort.
  /// </summary>
  [Command(Name = "merge-sort", Description = "Performs the merge sort.")]
  internal class CommandMergeSort
  {
    [Required]
    [Argument(0, "Input file path")]
    public string InFilePath { get; set; }

    [Option(LongName = "out", Description = "Output file path.")]
    public string OutFilePath { get; set; }

    [Option(LongName = "temp", Description = "Temp directory path.")]
    public string TempDirectoryPath { get; set; }

    public async Task OnExecuteAsync()
    {
      var fileSystem = new FileSystem();
      var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
      });

      var fsContextOptions = new FileContextOptions
      {
        InFilePath = this.InFilePath,
        OutFilePath = this.OutFilePath,
        TempDirectoryPath = this.TempDirectoryPath,
        UseOutFile = true
      };

      using(var fileContext = new FileContext(fileSystem, loggerFactory, fsContextOptions))
      {
        var options = new MergeSortOptions
        {
          MaxConcurrentJobs = Environment.ProcessorCount - 1 // Let user observe the perfmon
        };

        IMergeSortTask mergeSortTask = new MergeSortTaskV2();
        Console.WriteLine($"Launching merge sort V2...");
        await mergeSortTask.ExecuteAsync(fileContext, loggerFactory, options);
      }
    }
  }
}
