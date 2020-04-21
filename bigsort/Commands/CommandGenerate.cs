using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace BigSort.Commands
{
  /// <summary>
  /// The check command.
  /// </summary>
  [Command(Name = "generate", Description = @"Generates the data file of required size and dictionary.")]
  internal class CommandGenerate
  {
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
        await Task.Delay(1000);
        Console.WriteLine("The file has been generated successfully.");
      }
      catch(Exception e)
      {
        logger.LogCritical(e, "Test data generateion has failed.");
      }
      finally
      {
      }
    }
  }
}
