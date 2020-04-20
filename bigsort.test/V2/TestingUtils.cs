using System;
using System.Text;

namespace BigSort.Test.V2
{
  /// <summary>
  /// Common helpers for testing.
  /// </summary>
  public static class TestingUtils
  {
    public static long StringToInfix(string s)
    {
      var infixBytes = Encoding.Unicode.GetBytes(s);
      var result = BitConverter.ToInt64(infixBytes);

      return result;
    }

    public static string InfixToString(long infix)
    {
      var infixBytes = BitConverter.GetBytes(infix);
      var result = Encoding.Unicode.GetString(infixBytes);

      return result;
    }
  }
}
