using System;
using System.IO;
using System.Threading.Tasks.Dataflow;

namespace BigSort.V2
{
  /// <summary>
  /// The source data reader for V2.
  /// </summary>
  internal class SourceReader
  {
    public void Start(string inFilePath, ITargetBlock<StringBuffer> target)
    {
      using(StreamReader sr = File.OpenText(inFilePath))
      {
        var splitBufferSize = 1000000; // 19mb?
        var memBuffer = new string[splitBufferSize];

        string s = String.Empty;
        var splitBufferPos = 0;
        while((s = sr.ReadLine()) != null)
        {
          // Push the buffer to processing.
          // This may block the thread if we have too many concurrent sorting tasks.
          if(splitBufferPos == splitBufferSize)
          {
            var splitBuffer = new StringBuffer(memBuffer, splitBufferSize);
            target.Post(splitBuffer);

            memBuffer = new string[splitBufferSize];
            splitBufferPos = 0;
          }
          memBuffer[splitBufferPos] = s;
          splitBufferPos++;
        }

        // Sort the final buffer
        if(splitBufferPos > 0)
        {
          var splitBuffer = new StringBuffer(memBuffer, splitBufferSize);
          target.Post(splitBuffer);
        }

        target.Complete();
      }
    }
  }
}
