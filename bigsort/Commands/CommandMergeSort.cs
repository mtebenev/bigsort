using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace BigSort.Commands
{
  /// <summary>
  /// The command executes merge sort.
  /// </summary>
  [Command(Name = "merge-sort")]
  internal class CommandMergeSort
  {
    public Task OnExecuteAsync()
    {
      return Task.CompletedTask;
    }
  }
}
