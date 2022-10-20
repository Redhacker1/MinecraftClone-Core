using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using SilkyNvg.Rendering;
using Veldrid;

namespace NVGRenderer.Rendering.Calls
{
    internal class FillCall : Call
    {

        public FillCall(int image, StrokePath[] paths, uint triangleOffset, int uniformOffset, CompositeOperationState compositeOperation, NvgRenderer renderer)
            : base(image, paths, triangleOffset, 4, uniformOffset, default, PipelineSettings.Fill(compositeOperation), PipelineSettings.FillStencil(compositeOperation), PipelineSettings.FillEdgeAa(compositeOperation), renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls) 
        {

            Pipeline sPipeline = frame.PipelineCache.GetPipeLine(stencilPipeline, renderer); 
            ResourceSet descriptorSet =  frame.ResourceSetCache.GetResourceSet(new ResourceSetData
            {
                uniformOffset = uniformOffset,
                image = image
            });
            
            DrawCall call = new DrawCall
            {
                Pipeline = sPipeline,
                Set = descriptorSet
            };
            
            
            //cmd.SetPipeline(sPipeline);
            //cmd.SetGraphicsResourceSet(0, descriptorSet);
            //cmd.SetFramebuffer(renderer.Device.SwapchainFramebuffer);
            
            //cmd.SetGraphicsResourceSet(0, descriptorSet);
            foreach (StrokePath path in paths)
            {
                //cmd.Draw(path.FillCount, 1, path.FillOffset, 0);
                call.Offset = path.FillOffset;
                call.Count = path.FillCount;
                drawCalls.Add(call);
            }

            if (renderer.EdgeAntiAlias)
            {
                Pipeline aaPipeline = frame.PipelineCache.GetPipeLine(antiAliasPipeline, renderer);
                call.Pipeline = aaPipeline;
                //cmd.SetPipeline(aaPipeline);
                foreach (StrokePath path in paths)
                {
                    //cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                    call.Offset = path.StrokeOffset;
                    call.Count = path.StrokeCount;
                    drawCalls.Add(call);
                }
            }

            Pipeline fPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, renderer);
            call.Count = triangleCount;
            call.Offset = triangleOffset;
            call.Pipeline = fPipeline;
            //cmd.SetPipeline(fPipeline);
            //cmd.Draw(triangleCount, 1, triangleOffset, 0);
            
        }

    }
}