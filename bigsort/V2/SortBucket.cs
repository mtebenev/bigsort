using System.Collections.Generic;

namespace BigSort.V2
{
  /// <summary>
  /// The bucket contains strings with the same infix.
  /// </summary>
  internal class SortBucket
  {
    public SortBucket(uint infix, List<SortRecord> records)
    {
      this.Infix = infix;
      this.Records = records;
    }

    /// <summary>
    /// The idea here to speed up partially sorting by using the infix.
    /// Consider the following record: '1379090337. Banana'
    /// We treat first symbols of the word (the primary key) as the infix (i.e. 'Bana').
    /// This way we can quickly group the records by buckets.
    /// </summary>
    public uint Infix { get; }

    /// <summary>
    /// The bucket records.
    /// </summary>
    public List<SortRecord> Records { get; }
  }
}
