﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Threading.Tasks;
using BigSort.Common;
using BigSort.V2;
using BigSort.Validation;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace BigSort.Commands
{
  /// <summary>
  /// The check command.
  /// </summary>
  [Command(Name = "check", Description = @"The command performs simple check to ensure that the output file is correct.
Basically it just reads the output file generated by the merge-sort command and ensures that all lines are in the sorted order (the increasing sequence)
")]
  internal class CommandCheck
  {
    [Required]
    [Argument(0, "Input file path")]
    public string InFilePath { get; set; }

    public async Task OnExecuteAsync()
    {
      var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
      });
      var logger = loggerFactory.CreateLogger(nameof(CommandCheck));

      try
      {
        var fileSystem = new FileSystem();
        var fsContextOptions = new FileContextOptions
        {
          InFilePath = this.InFilePath,
          UseOutFile = false
        };

        using(var fileContext = new FileContext(fileSystem, loggerFactory, fsContextOptions))
        {
          var context = new PipelineContext(loggerFactory, fileContext);
          var sourceReader = new SourceReader();
          var checkOrderBlock = CheckOrderBlock.Create(fileContext.GetInFileSize());

          const int blockSize = 10000; // The block size in lines
          var readerTask = sourceReader.StartAsync(
            loggerFactory,
            fileContext.InFilePath,
            blockSize,
            context.Stats,
            checkOrderBlock
          );
          await Task.WhenAll(checkOrderBlock.Completion, readerTask);

          logger.LogInformation("The file is valid.");
        }
      }
      catch(Exception e)
      {
        logger.LogCritical(e, "The validation has been faulted");
      }
      finally
      {
      }
    }
  }
}
