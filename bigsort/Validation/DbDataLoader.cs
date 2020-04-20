using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Dapper;

namespace BigSort.Validation
{
  /// <summary>
  /// Loads database records by blocks.
  /// </summary>
  internal class DbDataLoader
  {
    public static Task StartAsync(IDbConnection connection, int pageSize, ITargetBlock<DataRecord[]> targetBlock)
    {
      var task = Task.Run(async () =>
      {
        var offset = 0;
        DataRecord[] records;

        do
        {
          var query = @"SELECT * FROM DataRecords ORDER BY DataStr ASC, DataNum ASC LIMIT @Offset, @PageSize";
          var recordsSource = await connection.QueryAsync<DataRecord>(query, new { Offset = offset, PageSize = pageSize });
          records = recordsSource.ToArray();
          if(records.Length > 0)
          {
            targetBlock.Post(records);
          }
          offset += pageSize;
        } while(records != null && records.Length > 0);
      });

      return task;
    }
  }
}
