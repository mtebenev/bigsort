using System;
using System.Threading.Tasks;
using BigSort.Commands;
using McMaster.Extensions.CommandLineUtils;
using StackExchange.Profiling;

namespace bigsort
{
  [Command("bigsort")]
  [Subcommand(
    typeof(CommandMergeSort)
  )]
  class Program
  {
    static async Task<int> Main(string[] args)
    {
      var profiler = MiniProfiler.StartNew();
      var result = await CommandLineApplication.ExecuteAsync<Program>(args);
      profiler.Stop();

      var executionTime = TimeSpan.FromMilliseconds((double)profiler.DurationMilliseconds);
      Console.WriteLine(profiler.RenderPlainText());
      Console.WriteLine($"Finished execution: {executionTime}");

      return result;
    }
  }
}
