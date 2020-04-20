using BigSort.V2;
using Xunit;

namespace BigSort.Test.V2
{
  public class SortRecordComparerTest
  {
    [Fact]
    public void Should_Compare_String_Part()
    {
      var r1 = new SortRecord("1. aaaa");
      var r2 = new SortRecord("1. bbbb");

      var comparer = new SortRecordComparer();
      Assert.True(comparer.Compare(r1, r2) < 0);
    }

    [Fact]
    public void Should_Consider_String_Part_First()
    {
      var r1 = new SortRecord("2. aaaa");
      var r2 = new SortRecord("1. bbbb");

      var comparer = new SortRecordComparer();
      Assert.True(comparer.Compare(r1, r2) < 0);
    }

    [Fact]
    public void Should_Compare_Number_Part()
    {
      var r1 = new SortRecord("2. aaaa");
      var r2 = new SortRecord("1. aaaa");

      var comparer = new SortRecordComparer();
      Assert.True(comparer.Compare(r1, r2) > 0);
    }
  }
}
