﻿using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Pipelines;

namespace NVGRenderer.Rendering.Calls
{
    public abstract class Call
    {

        protected readonly int image;
        protected readonly StrokePath[] paths;
        protected readonly uint triangleOffset;
        protected readonly uint triangleCount;
        protected readonly int uniformOffset;
        
        protected readonly PipelineSettings renderPipeline;
        protected readonly PipelineSettings stencilPipeline;
        protected readonly PipelineSettings antiAliasPipeline;

        protected readonly NvgRenderer _renderer;

        protected Call(int image, StrokePath[] paths, uint triangleOffset, uint triangleCount, int uniformOffset,
            PipelineSettings renderPipeline, PipelineSettings stencilPipeline, PipelineSettings antiAliasPipeline, NvgRenderer renderer)
        {
            this.image = image;
            this.paths = paths;
            this.triangleOffset = triangleOffset;
            this.triangleCount = triangleCount;
            this.uniformOffset = uniformOffset;
            this.renderPipeline = renderPipeline;
            this.stencilPipeline = stencilPipeline;
            this.antiAliasPipeline = antiAliasPipeline;
            this._renderer = renderer;
        }

        public abstract void Run(NvgFrame frame, List<DrawCall> drawCalls);
    }
}