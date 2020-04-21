using System;
using System.IO;
using System.Threading.Tasks;
using BigSort.Common;
using BigSort.V2;
using McMaster.Extensions.CommandLineUtils;

namespace BigSort.Commands
{
  /// <summary>
  /// The command executes merge sort.
  /// </summary>
  [Command(Name = "merge-sort", Description = "Performs the merge sort.")]
  internal class CommandMergeSort
  {

    public async Task OnExecuteAsync()
    {
      var tempPath = @"c:\_sorting\chunks";
      if(Directory.Exists(tempPath))
      {
        Directory.Delete(tempPath, true);
      }
      Directory.CreateDirectory(tempPath);

      var options = new MergeSortOptions
      {
        InFilePath = @"c:\_sorting\file.txt",
        OutFilePath = @"c:\_sorting\out.txt",
        TempDirectoryPath = @"C:\_sorting\chunks",
        MaxConcurrentJobs = Environment.ProcessorCount - 1 // Let user observe the perfmon
      };

      IMergeSortTask mergeSortTask = new MergeSortTaskV2();
      Console.WriteLine($"Launching merge sort V2...");
      await mergeSortTask.ExecuteAsync(options);
    }
  }
}
