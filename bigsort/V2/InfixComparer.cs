﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BigSort.V2
{
  /// <summary>
  /// Sorts infixes in the alphabetical order.
  /// </summary>
  internal class InfixComparer : IComparer<ushort>
  {
    /// <summary>
    /// IComparer.
    /// </summary>
    public int Compare([AllowNull] ushort x, [AllowNull] ushort y)
    {
      var infix1 = Encoding.Unicode.GetString(BitConverter.GetBytes(x));
      var infix2 = Encoding.Unicode.GetString(BitConverter.GetBytes(y));

      return string.CompareOrdinal(infix1, infix2);
    }
  }
}
