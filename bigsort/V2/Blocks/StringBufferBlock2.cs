using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using BigSort.V3.Events;
using Microsoft.Extensions.Logging;

namespace BigSort.V2.Blocks
{
  /// <summary>
  /// The buffer block for buffered reading
  /// </summary>
  internal class StringBufferBlock2
  {
    private readonly Dictionary<ushort, SortThresholdBuffer> _buckets;
    private readonly IPipelineContext _pipelineContext;
    private readonly ILogger _logger;
    private readonly int _thresholdSize;

    /// <summary>
    /// Ctor.
    /// </summary>
    private StringBufferBlock2(IPipelineContext pipelineContext, int bufferSize)
    {
      this._thresholdSize = bufferSize / 8; // Empirical value. There should be a balance between memory consumption and merge speed.
      this._buckets = new Dictionary<ushort, SortThresholdBuffer>();
      this._pipelineContext = pipelineContext;
      this._logger = pipelineContext.LoggerFactory.CreateLogger(nameof(Blocks.StringBufferBlock));
    }

    /// <summary>
    /// The factory.
    /// </summary>
    public static TransformManyBlock<BufferReadEvent3, SortChunkBuffer> Create(IPipelineContext pipelineContext, int bufferSize)
    {
      var block = new StringBufferBlock2(pipelineContext, bufferSize);
      var result = new TransformManyBlock<BufferReadEvent3, SortChunkBuffer>(
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
    public IEnumerable<SortChunkBuffer> Execute(BufferReadEvent3 evt)
    {
      this._logger.LogInformation("Start string buffer processing.");
      var flushedBuffers = this.PushNewRecords(evt);

      // If the source reading done, flush all the buffers.
      if(evt.IsReadCompleted)
      {
        flushedBuffers = this._buckets
          .Select(kvp => kvp.Value.DetachChunkBuffer())
          .ToList();
      }

      this.ProcessFlushedChunks(flushedBuffers, evt.IsReadCompleted);
      evt.Dispose(); // The read strings buffer ends here.

      return flushedBuffers;
    }

    /// <summary>
    /// Pushes all newly obtained records the buckets.
    /// </summary>
    private List<SortChunkBuffer> PushNewRecords(BufferReadEvent3 evt)
    {
      var flushedBuffers = new List<SortChunkBuffer>();

      var stringSource = evt.EnumerateStrings();
      foreach(var s in stringSource)
      {
        var sr = new SortRecord(s);
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
