using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using SilkyNvg.Rendering;
using Veldrid;
using Path = NVGRenderer.Rendering.StrokePath;

namespace NVGRenderer.Rendering.Calls
{
    internal class StrokeCall : Call
    {

        public StrokeCall(int image, Path[] paths, int uniformOffset, CompositeOperationState compositeOperation, NvgRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, default, PipelineSettings.Stroke(compositeOperation), default, default, renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {

            Pipeline fillPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, renderer);
            ResourceSet descriptorSet = frame.ResourceSetCache.GetResourceSet(new ResourceSetData
            {
                image = image,
                uniformOffset = uniformOffset

            });
            
            DrawCall call = new DrawCall
            {
                Pipeline = fillPipeline,
                Set = descriptorSet
            };

            //cmd.SetPipeline(fillPipeline);
            //cmd.SetFramebuffer(renderer.Device.SwapchainFramebuffer);
            
            
            //cmd.SetGraphicsResourceSet(0, descriptorSet);
            

            foreach (Path path in paths)
            {
                //cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                call.Offset = path.StrokeOffset;
                call.Count = path.StrokeCount;
                drawCalls.Add(call);
            }

        }

    }
}