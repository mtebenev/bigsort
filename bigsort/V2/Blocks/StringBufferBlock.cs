using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2.Events;
using Microsoft.Extensions.Logging;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// The block is responsible for bucket collecting records into the buckets.
  /// We divide the input strings by the 'infix' - the first letter of the string part of the record.
  /// This way we can quickly group the records by buckets and later merge them in parallel.
  /// </summary>

  /// </summary>
  internal class StringBufferBlock
  {
    private readonly Dictionary<ushort, SortThresholdBuffer> _buckets;
    private readonly IPipelineContext _pipelineContext;
    private readonly ILogger _logger;
    private readonly int _thresholdSize;

    /// <summary>
    /// Ctor.
    /// </summary>
    private StringBufferBlock(IPipelineContext pipelineContext, int bufferSize)
    {
      this._thresholdSize = bufferSize / 8; // Empirical value. There should be a balance between memory consumption and merge speed.
      this._buckets = new Dictionary<ushort, SortThresholdBuffer>();
      this._pipelineContext = pipelineContext;
      this._logger = pipelineContext.LoggerFactory.CreateLogger(nameof(Blocks.StringBufferBlock));
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static TransformManyBlock<BufferReadEvent, SortChunkBuffer> Create(IPipelineContext pipelineContext, int bufferSize)
    {
      var block = new StringBufferBlock(pipelineContext, bufferSize);
      var result = new TransformManyBlock<BufferReadEvent, SortChunkBuffer>(
        (evt) => block.Execute(evt),
        new ExecutionDataflowBlockOptions
        {
          // BoundedCapacity is essential to block the reader until we process current messages,
          // otherwise the memory usage grows because the reader continues producing data.
          // Setting the bound capacity we are effectively blocking the reading to let us 'swallow' the current data.
          BoundedCapacity = 2,
          MaxDegreeOfParallelism = 1
        });

      return result;
    }

    /// <summary>
    /// Executes the bucket sort on the string buffer.
    /// </summary>
    public IEnumerable<SortChunkBuffer> Execute(BufferReadEvent evt)
    {
      this._logger.LogInformation("Start string buffer processing.");
      var flushedBuffers = this.PushNewRecords(evt.Buffer);

      // If the source reading done, flush all the buffers.
      if(evt.IsReadCompleted)
      {
        flushedBuffers = this._buckets
          .Select(kvp => kvp.Value.DetachChunkBuffer())
          .ToList();
      }

      this.ProcessFlushedChunks(flushedBuffers, evt.IsReadCompleted);
      evt.Buffer.Dispose(); // The read strings buffer ends here.

      return flushedBuffers;
    }

    /// <summary>
    /// Pushes all newly obtained records the buckets.
    /// </summary>
    private List<SortChunkBuffer> PushNewRecords(StringBuffer stringBuffer)
    {
      var flushedBuffers = new List<SortChunkBuffer>();

      for(int i = 0; i < stringBuffer.ActualSize; i++)
      {
        var sr = new SortRecord(stringBuffer.Buffer[i]);
        SortThresholdBuffer thresholdBuffer;
        if(!this._buckets.TryGetValue(sr.Infix, out thresholdBuffer))
        {
          thresholdBuffer = SortThresholdBuffer.Allocate(sr.Infix, this._thresholdSize);
          this._buckets[sr.Infix] = thresholdBuffer;
        }
        else
        {
          if(!thresholdBuffer.CanAdd())
          {
            flushedBuffers.Add(thresholdBuffer.DetachChunkBuffer());
            thresholdBuffer = SortThresholdBuffer.Allocate(sr.Infix, this._thresholdSize);
            this._buckets[sr.Infix] = thresholdBuffer;
          }
        }

        thresholdBuffer.Add(sr);
      }

      return flushedBuffers;
    }

    private void ProcessFlushedChunks(List<SortChunkBuffer> flushedBuffers, bool isReadingCompleted)
    {
      foreach(var fb in flushedBuffers)
      {
        this._pipelineContext.OnReceivedChunk(fb.Infix);
        this._logger.LogDebug("Flushed block, infix: {infix}", InfixUtils.InfixToString(fb.Infix));
      }

      if(isReadingCompleted)
      {
        this._pipelineContext.OnInfixesReady();
      }
    }

  }
}
