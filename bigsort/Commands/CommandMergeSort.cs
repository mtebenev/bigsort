﻿using System;
using System.IO;
using System.Threading.Tasks;
using BigSort.Common;
using BigSort.V1;
using BigSort.V2;
using McMaster.Extensions.CommandLineUtils;

namespace BigSort.Commands
{
  internal enum MergeType
  {
    V1,
    V2
  }

  /// <summary>
  /// The command executes merge sort.
  /// </summary>
  [Command(Name = "merge-sort")]
  internal class CommandMergeSort
  {
    /// <summary>
    /// Merge type (algorithm).
    /// </summary>
    [Option(LongName = "type", ShortName = "t")]
    public (bool HasValue, MergeType MergeType) MergeTypeParam { get; set; }

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

      IMergeSortTask mergeSortTask;
      if(this.MergeTypeParam.HasValue && this.MergeTypeParam.MergeType == MergeType.V1)
      {
        Console.WriteLine("Doin V1");
        mergeSortTask = new MergeSortTaskV1();
      }
      else
      {
        Console.WriteLine("Doin V2");
        mergeSortTask = new MergeSortTaskV2();
      }

      await mergeSortTask.ExecuteAsync(options);
    }
  }
}
