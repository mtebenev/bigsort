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
    public static ushort StringToInfix(string s)
    {
      var infixBytes = Encoding.Unicode.GetBytes(s);
      var result = BitConverter.ToUInt16(infixBytes);

      return result;
    }

    /// <summary>
    /// Infix -> string conversion.
    /// </summary>
    public static string InfixToString(ushort infix)
    {
      var infixBytes = BitConverter.GetBytes(infix);
      var result = Encoding.Unicode.GetString(infixBytes);

      return result;
    }
  }
}
