using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using BigSort.Common;
using MoreLinq;

namespace BigSort.V2
{
  /// <summary>
  /// Merges the bucket files with k-way merge.
  /// </summary>
  internal static class BucketMerger
  {
    /// <summary>
    /// k-way file merge.
    /// </summary>
    public static void MergeKWay(IFileSystem fileSystem, IList<string> filePaths, string outFilePath)
    {
      var sources = filePaths
         .Select(p => fileSystem.File
         .ReadLines(p).Select(l => new SortRecord(l)))
         .ToList();

      var e1 = sources[0];
      var erest = sources.Skip(1).ToArray();
      var comparer = new SortRecordComparer();
      var target = e1
        .SortedMerge(OrderByDirection.Ascending, comparer, erest)
        .Select(sr => sr.Value);
      fileSystem.File.WriteAllLines(outFilePath, target);
    }

    /// <summary>
    /// Pairwise merging. Takes sources files by pairs and merges to the output file.
    /// </summary>
    public static Task<string> MergePairwiseAsync(IList<string> filePaths, IFileContext fileContext)
    {
      var levelPaths = filePaths.ToList();
      while(levelPaths.Count > 1)
      {
        // Canonically (in the tape era) the merge sort operates with power of two runs (2, 4, 8 etc).
        // Nowadays we have SDDs, so let's be a bit funky. Let's accept any number of chunks.
        var skipFirst = levelPaths.Count % 2 != 0;
        var thisLevelPaths = levelPaths
          .Skip(skipFirst ? 1 : 0)
          .Pairwise((p1, p2) => (p1, p2))
          .Where((_, idx) => idx % 2 == 0)
          .ToList();
        var nextLevel = new List<string>();
        if(skipFirst)
        {
          nextLevel.Add(levelPaths[0]);
        }

        thisLevelPaths.AsParallel().ForAll(async (paths) =>
        {
          var nextPath = await MergeFilesAsync(paths.p1, paths.p2, fileContext);
          nextLevel.Add(nextPath);
        });

        levelPaths = nextLevel;
      }

      return Task.FromResult(levelPaths[0]);
    }

    private static Task<string> MergeFilesAsync(string path1, string path2, IFileContext fileContext)
    {
      var enu1 = File.ReadLines(path1)
        .Select(s => new SortRecord(s));
      var enu2 = File.ReadLines(path2)
        .Select(s => new SortRecord(s));

      var outFileName = fileContext.AddTempFile(null);

      var comparer = new SortRecordComparer();
      var mergeEnumeragele = enu1.SortedMerge(OrderByDirection.Ascending, comparer, enu2)
        .Select(sr => sr.Value);
      File.WriteAllLines(outFileName, mergeEnumeragele);

      return Task.FromResult(outFileName);
    }
  }
}
