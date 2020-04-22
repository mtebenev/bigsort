using System;
using System.Data;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;

namespace BigSort.Validation
{
  /// <summary>
  /// Pushes records into the database for testing.
  /// </summary>
  internal class DbPusherBlock
  {
    /// <summary>
    /// The factory.
    /// </summary>
    public static ITargetBlock<BufferReadEvent> Create(ILoggerFactory loggerFactory, IDbConnection connection, long fileSize)
    {
      var logger = loggerFactory.CreateLogger(nameof(DbPusherBlock));
      var progressCounter = new FileProgressCounter(fileSize);
      var dbPusherBlock = new ActionBlock<BufferReadEvent>(evt =>
      {
        DbPusherBlock.InsertBlock(logger, connection, evt, progressCounter);
      });

      return dbPusherBlock;
    }

    private static void InsertBlock(ILogger logger, IDbConnection connection, BufferReadEvent evt, FileProgressCounter progressCounter)
    {
      try
      {
        logger.LogInformation("Inserting lines block to sqlite database...");
        connection.Open();
        using(var transaction = connection.BeginTransaction())
        {
          var r = new DataRecord();
          int dotPos;
          for(int i = 0; i < evt.Buffer.BufferSize; i++)
          {
            if(i > 0 && i % 100000 == 0)
            {
              logger.LogDebug($"Progress: {progressCounter.GetProgressText()}");
            }

            dotPos = evt.Buffer.Buffer[i].IndexOf('.');
            var spanNum = evt.Buffer.Buffer[i].AsSpan(0, dotPos); // number span

            r.DataNum = int.Parse(spanNum);
            r.DataStr = evt.Buffer.Buffer[i].Substring(dotPos + 1);

            connection.Insert(r, transaction);
            progressCounter.OnLineProcessed(evt.Buffer.Buffer[i]);
          }
          transaction.Commit();
        }
      }
      finally
      {
        connection.Close();
      }
    }
  }
}
