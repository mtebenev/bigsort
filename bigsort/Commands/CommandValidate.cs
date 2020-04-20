using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2;
using BigSort.V2.Events;
using BigSort.Validation;
using Dapper;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace BigSort.Commands
{
  /// <summary>
  /// The command validates sorted files using sqlite.
  /// </summary>
  [Command(Name = "validate")]
  internal class CommandValidate
  {
    public async Task OnExecuteAsync()
    {

      var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug);
      });

      var inFilePath = @"C:\_sorting\file.txt";
      var outFilePath = @"C:\_sorting\out.txt";
      var tempPath = @"c:\_sorting";
      var databaseName = "temp.db";
      var dbPath = Path.Combine(tempPath, databaseName);

      if(File.Exists(dbPath))
      {
        File.Delete(dbPath);
      }

      var connection = this.CreateDatabase(dbPath);
      var reader = new SourceReader();

      // Push records in db
      var context = new PipelineContext(loggerFactory);
      var dbPusherBlock = DbPusherBlock.Create(connection);
      reader.Start(inFilePath, context, dbPusherBlock);
      await dbPusherBlock.Completion;

      // Compare database with out file
      await this.CompareAsync(connection, context, outFilePath);

      connection.Close();
    }

    /// <summary>
    /// Initializes the database.
    /// </summary>
    private IDbConnection CreateDatabase(string dbPath)
    {
      var connectionString = new SqliteConnectionStringBuilder
      {
        DataSource = dbPath,
      }.ToString();

      var connection = new SqliteConnection(connectionString);
      var queryTable = @"CREATE TABLE DataRecords (
                        DataNum   INTEGER        NOT NULL,
                        DataStr   NVARCHAR (256) NOT NULL
);";
      
      connection.Execute(queryTable);

      var queryIdx1 = "CREATE INDEX data_records_str_idx ON DataRecords (DataStr);";
      connection.Execute(queryIdx1);
      var queryIdx2 = "CREATE INDEX data_records_num_idx ON DataRecords (DataNum);";
      connection.Execute(queryIdx2);

      return connection;
    }

    /// <summary>
    /// Compares records in db with out file.
    /// </summary>
    private async Task CompareAsync(IDbConnection connection, IPipelineContext pipelineContext, string checkFilePath)
    {
      const int pageSize = 10000;
      var joinBlock = new JoinBlock<DataRecord[], BufferReadEvent>();

      // Compare the records.
      var comparisonBlock = ComparisonBlock.Create();
      joinBlock.LinkTo(comparisonBlock);

      // Read db records
      var dbLoaderTask = DbDataLoader.StartAsync(connection, pageSize, joinBlock.Target1);

      // Read the file
      var sourceReader = new SourceReader();
      var readerTask = sourceReader.StartAsync(checkFilePath, pipelineContext, joinBlock.Target2);

      await Task.WhenAll(dbLoaderTask, readerTask, comparisonBlock.Completion);
    }
  }
}
