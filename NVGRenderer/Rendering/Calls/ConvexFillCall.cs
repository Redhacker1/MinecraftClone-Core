using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using Veldrid;

namespace NVGRenderer.Rendering.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int image, StrokePath[] paths, ulong uniformOffset, CompositeOperationState op, NvgRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.ConvexFill(op, renderer.TriangleListFill), default, PipelineSettings.ConvexFillEdgeAa(op), renderer) { }

        public override unsafe void Run(Frame frame, CommandList cmd)
        {
            
            Pipeline fPipeline = PipelineCache.GetPipeLine(renderPipeline, renderer);
            cmd.SetPipeline(fPipeline);
            cmd.SetFramebuffer(renderer._device.SwapchainFramebuffer);
            
            renderer.Shader.SetUniforms(frame, out ResourceSet descriptorSet, uniformOffset, image);
            cmd.SetGraphicsResourceSet(0, descriptorSet);

            foreach (StrokePath path in paths)
            {
                cmd.Draw(path.FillCount, 1, path.FillOffset, 0);
            }

            if (renderer.EdgeAntiAlias)
            {
                Pipeline aaPipeline = PipelineCache.GetPipeLine(renderPipeline, renderer); 
                cmd.SetPipeline(aaPipeline);

                foreach (StrokePath path in paths)
                {
                    cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                }
            }
        }

    }
}