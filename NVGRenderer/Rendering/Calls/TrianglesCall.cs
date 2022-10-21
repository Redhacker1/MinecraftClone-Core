using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using SilkyNvg.Rendering;
using Veldrid;

namespace NVGRenderer.Rendering.Calls
{
    internal class TrianglesCall : Call
    {

        public TrianglesCall(int image, CompositeOperationState compositeOperation, uint triangleOffset, uint triangleCount, int uniformOffset, NvgRenderer renderer)
            : base(image, null, triangleOffset, triangleCount, uniformOffset, default, PipelineSettings.Triangles(compositeOperation), default, default, renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {
            
            Pipeline pipeline = frame.PipelineCache.GetPipeLine(renderPipeline, renderer);

            DrawCall call = new DrawCall
            {
                Pipeline = pipeline,
                Set = new ResourceSetData
                {
                    uniformOffset = uniformOffset,
                    image = image
                },
                Count = triangleCount,
                Offset = triangleOffset,
            };
            
            drawCalls.Add(call);
            
            //cmd.SetFramebuffer(renderer.Device.SwapchainFramebuffer);
            //cmd.SetPipeline(pipeline);
            //cmd.SetGraphicsResourceSet(0, descriptorSet);
            //cmd.Draw(triangleCount, 1, triangleOffset, 0);

        }

    }
}