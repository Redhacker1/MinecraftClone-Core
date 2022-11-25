using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using Veldrid;
using Path = NVGRenderer.Rendering.StrokePath;

namespace NVGRenderer.Rendering.Calls
{
    internal class StencilStrokeCall : Call
    {

        public StencilStrokeCall(int image, Path[] paths, int uniformOffset, CompositeOperationState compositeOperation, NvgRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset,  PipelineSettings.StencilStroke(compositeOperation), PipelineSettings.StencilStrokeStencil(compositeOperation), PipelineSettings.StencilStrokeEdgeAA(compositeOperation), renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {


            Pipeline sPipeline = frame.PipelineCache.GetPipeLine(stencilPipeline, _renderer, frame);

            DrawCall call = new DrawCall
            {
                Pipeline = sPipeline,
                Set = new ResourceSetData
                {
                    image = image
                },
                UniformOffset = (uint)uniformOffset
            };
            
            foreach (Path path in paths)
            {
                call.Offset = path.StrokeOffset;
                call.Count = path.StrokeCount;
                drawCalls.Add(call);
            }

            Pipeline aaPipeline = frame.PipelineCache.GetPipeLine(antiAliasPipeline, _renderer, frame);
            call.Pipeline = aaPipeline;


            call.Set = new ResourceSetData
            {
                image = image
            };
            call.UniformOffset = (uint) uniformOffset;
            
            foreach (Path path in paths)
            {
                call.Offset = path.StrokeOffset;
                call.Count = path.StrokeCount;
                drawCalls.Add(call);
            }

            Pipeline stPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer, frame);
            call.Pipeline = stPipeline;
            
            foreach (Path path in paths)
            {
                call.Offset = path.StrokeOffset;
                call.Count = path.StrokeCount;
                drawCalls.Add(call);
            }
            
        }

    }
}