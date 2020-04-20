using System;
using System.Data;
using System.Threading.Tasks.Dataflow;
using BigSort.V2.Events;
using Dapper.Contrib.Extensions;

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
    public static ITargetBlock<BufferReadEvent> Create(IDbConnection connection)
    {
      var dbPusherBlock = new ActionBlock<BufferReadEvent>(evt =>
      {
        DbPusherBlock.InsertBlock(connection, evt);
      });

      return dbPusherBlock;
    }

    private static void InsertBlock(IDbConnection connection, BufferReadEvent evt)
    {
      try
      {
        Console.WriteLine("Inserting block...");
        connection.Open();
        using(var transaction = connection.BeginTransaction())
        {
          var r = new DataRecord();
          int dotPos;
          for(int i = 0; i < evt.Buffer.BufferSize; i++)
          {
            dotPos = evt.Buffer.Buffer[i].IndexOf('.');
            var spanNum = evt.Buffer.Buffer[i].AsSpan(0, dotPos); // number span

            r.DataNum = int.Parse(spanNum);
            r.DataStr = evt.Buffer.Buffer[i].Substring(dotPos + 1);

            connection.Insert(r, transaction);

            if(i % 100000 == 0)
            {
              Console.WriteLine($"Inserted {i} rows.");
            }
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
