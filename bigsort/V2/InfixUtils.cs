using System;
using System.Text;

namespace BigSort.V2
{
  /// <summary>
  /// Infix <-> string conversion.
  /// </summary>
  internal static class InfixUtils
  {
    /// <summary>
    /// String -> infix conversion.
    /// </summary>
    public static uint StringToInfix(string s)
    {
      var infixBytes = Encoding.Unicode.GetBytes(s);
      var result = BitConverter.ToUInt32(infixBytes);

      return result;
    }

    /// <summary>
    /// Infix -> string conversion.
    /// </summary>
    public static string InfixToString(uint infix)
    {
      var infixBytes = BitConverter.GetBytes(infix);
      var result = Encoding.Unicode.GetString(infixBytes);

      return result;
    }
  }
}
