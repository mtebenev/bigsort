using System;

namespace BigSort.Common
{
  /// <summary>
  /// String utils.
  /// </summary>
  internal static class StringUtils
  {
    private static string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    /// <summary>
    /// http://csharphelper.com/blog/2017/03/parse-file-sizes-in-kb-mb-gb-and-so-forth-in-c/
    /// </summary>
    public static double ParseFileSize(string value, int kb_value)
    {
      // Remove leading and trailing spaces.
      value = value.Trim();

      try
      {
        // Find the last non-alphabetic character.
        int ext_start = 0;
        for(int i = value.Length - 1; i >= 0; i--)
        {
          // Stop if we find something other than a letter.
          if(!char.IsLetter(value, i))
          {
            ext_start = i + 1;
            break;
          }
        }

        // Get the numeric part.
        double number = double.Parse(value.Substring(0, ext_start));

        // Get the extension.
        string suffix;
        if(ext_start < value.Length)
        {
          suffix = value.Substring(ext_start).Trim().ToUpper();
          if(suffix == "BYTES")
            suffix = "bytes";
        }
        else
        {
          suffix = "bytes";
        }

        // Find the extension in the list.
        int suffix_index = -1;
        for(int i = 0; i < SizeSuffixes.Length; i++)
        {
          if(SizeSuffixes[i] == suffix)
          {
            suffix_index = i;
            break;
          }
        }
        if(suffix_index < 0)
          throw new FormatException(
              "Unknown file size extension " + suffix + ".");

        // Return the result.
        return Math.Round(number * Math.Pow(kb_value, suffix_index));
      }
      catch(Exception ex)
      {
        throw new FormatException("Invalid file size format", ex);
      }
    }
  }
}
