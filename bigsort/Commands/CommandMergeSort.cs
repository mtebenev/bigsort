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
  [Command(Name = "merge-sort")]
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
        TempDirectoryPath = @"C:\_sorting\chunks"
      };

      var mergeSortTask = new MergeSortTaskV2();
      await mergeSortTask.ExecuteAsync(options);
    }
  }
}
