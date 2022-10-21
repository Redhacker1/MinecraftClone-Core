using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using SilkyNvg.Rendering;
using Veldrid;

namespace NVGRenderer.Rendering.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int image, StrokePath[] paths, int uniformOffset, CompositeOperationState op, NvgRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, default, PipelineSettings.ConvexFill(op), default, PipelineSettings.ConvexFillEdgeAa(op), renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {
            
            Pipeline fPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, renderer);

            DrawCall call = new DrawCall
            {
                Pipeline = fPipeline,
                Set = new ResourceSetData
                {
                    uniformOffset = uniformOffset,
                    image = image
                }
            };
            
            
            
            //cmd.SetPipeline(fPipeline);
            //cmd.SetFramebuffer(renderer.Device.SwapchainFramebuffer);
            //cmd.SetGraphicsResourceSet(0, descriptorSet);

            foreach (StrokePath path in paths)
            {
                call.Count = path.FillCount;
                call.Offset = path.FillOffset;
                drawCalls.Add(call);
            }

            if (renderer.EdgeAntiAlias)
            {
                Pipeline aaPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, renderer); 
                //cmd.SetPipeline(aaPipeline);
                call.Pipeline = aaPipeline;
                foreach (StrokePath path in paths)
                {
                    //cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                    call.Count = path.StrokeCount;
                    call.Offset = path.StrokeOffset;
                }
            }
            
        }

    }
}