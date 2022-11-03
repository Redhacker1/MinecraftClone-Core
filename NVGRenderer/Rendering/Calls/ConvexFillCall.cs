using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using Veldrid;

namespace NVGRenderer.Rendering.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int image, StrokePath[] paths, int uniformOffset, CompositeOperationState op, NvgRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.ConvexFill(op), default, PipelineSettings.ConvexFillEdgeAA(op), renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {
            
            Pipeline fPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer);

            DrawCall call = new DrawCall
            {
                Pipeline = fPipeline,
                Set = new ResourceSetData
                {
                    image = image
                },
                UniformOffset = (uint)uniformOffset
            };

            foreach (StrokePath path in paths)
            {
                call.Count = path.FillCount;
                call.Offset = path.FillOffset;
                drawCalls.Add(call);
            }

            if (_renderer.EdgeAntiAlias)
            {
                Pipeline aaPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer);
                call.Pipeline = aaPipeline;
                foreach (StrokePath path in paths)
                {
                    call.Count = path.StrokeCount;
                    call.Offset = path.StrokeOffset;
                }
            }
            
        }

    }
}