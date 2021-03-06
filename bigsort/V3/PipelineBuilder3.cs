﻿using System.Threading.Tasks.Dataflow;
using BigSort.Common;
using BigSort.V2;
using BigSort.V3.Blocks;
using BigSort.V3.Events;

namespace BigSort.V3
{
  /// <summary>
  /// The pipeline builder for v3
  /// </summary>
  internal class PipelineBuilder3
  {
    /// <summary>
    /// Builder method.
    /// </summary>
    public static (ITargetBlock<BufferReadEvent3>, TransformBlock<BufferReadEvent3, string>) Build(MergeSortOptions options, IPipelineContext pipelineContext, int bufferSize)
    {
      var bufferSortBlock = BufferSortBlock.Create(pipelineContext);
      return (bufferSortBlock, bufferSortBlock);
    }
  }
}
