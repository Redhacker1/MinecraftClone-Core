using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using Veldrid;
using Path = NVGRenderer.Rendering.StrokePath;

namespace NVGRenderer.Rendering.Calls
{
    internal class StrokeCall : Call
    {

        public StrokeCall(int image, Path[] paths, int uniformOffset, CompositeOperationState compositeOperation, NvgRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.Stroke(compositeOperation), default, default, renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {

            Pipeline fillPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer);

            DrawCall call = new DrawCall
            {
                Pipeline = fillPipeline,
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

        }

    }
}