using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BigSort.Common;
using BigSort.Generation;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace BigSort.Commands
{
  /// <summary>
  /// Test data generator types.
  /// </summary>
  internal enum GeneratorType
  {
    /// <summary>
    /// Simple word dictionary (fast).
    /// </summary>
    Dict1,

    /// <summary>
    /// Dictionary with permutations.
    /// </summary>
    Dict2,

    /// <summary>
    /// Random (2-3 words with 1-8 symbols) (slow).
    /// </summary>
    Random
  }


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

    /// <summary>
    /// Merge type (algorithm).
    /// </summary>
    [Option(LongName = "type", ShortName = "t", Description = @"The generator type:
dict1: (default) a fixed word dictionary;
dict2: the word dictionary with permutations;
random: (slow) randomly generated 2-3 words per line.
")]
    public (bool HasValue, GeneratorType GeneratorType) GeneratorTypeParam { get; set; }

    public async Task OnExecuteAsync()
    {

      var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug);
      });
      var logger = loggerFactory.CreateLogger(nameof(CommandGenerate));
      StreamWriter streamWriter = null;

      try
      {
        var limitSize = string.IsNullOrEmpty(this.Limit) ? "1GB" : this.Limit;
        var toGenerate = (long)StringUtils.ParseFileSize(limitSize, 1024);
        logger.LogInformation($"Generating file, size=~{limitSize}");

        streamWriter = new StreamWriter(this.OutFilePath, false);
        Func<ILineGenerator> lineGeneratorFactory = this.CreateLineGeneratorFactory(logger);

        var finalBlock = TestDataGenerator.Start(toGenerate, lineGeneratorFactory, streamWriter);
        await finalBlock.Completion;

        streamWriter.Flush();

        logger.LogInformation("The file has been generated successfully.");
      }
      catch(Exception e)
      {
        logger.LogCritical(e, "Test data generateion has failed.");
      }
      finally
      {
        streamWriter?.Dispose();
      }
    }

    /// <summary>
    /// Creates the line generator depending on the command options.
    /// </summary>
    private Func<ILineGenerator> CreateLineGeneratorFactory(ILogger logger)
    {
      Func<ILineGenerator> generatorFactory;
      var generatorType = this.GeneratorTypeParam.HasValue
        ? this.GeneratorTypeParam.GeneratorType
        : GeneratorType.Dict1;

      var dict = new[] { "Apple", "Banana", "Canon", "Dominant", "Ellipse", "Frozen", "Gilbert", "Hannover", "Gazelle", "Katana", "Limonade" };

      switch(generatorType)
      {
        case GeneratorType.Dict2:
          var permutations = dict
            .Take(5)  // Kudos, Dave!
            .Permutations()
            .Select(s => String.Join(' ', s))
            .ToArray();
          generatorFactory = () => new LineGeneratorDictionary(permutations);
          break;
        case GeneratorType.Random:
          generatorFactory = () => new LineGeneratorRandom();
          break;
        default:
          generatorFactory = () => new LineGeneratorDictionary(dict);
          break;
      }

      logger.LogInformation("Using generator: {generatorType}", generatorType);
      return generatorFactory;
    }
  }
}
