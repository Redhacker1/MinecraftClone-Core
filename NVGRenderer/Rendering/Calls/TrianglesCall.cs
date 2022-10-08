using NVGRenderer.Rendering.Pipelines;
using Veldrid;

namespace NVGRenderer.Rendering.Calls
{
    internal class TrianglesCall : Call
    {

        public TrianglesCall(int image, SilkyNvg.Blending.CompositeOperationState compositeOperation, uint triangleOffset, uint triangleCount, ulong uniformOffset, NvgRenderer renderer)
            : base(image, null, triangleOffset, triangleCount, uniformOffset, PipelineSettings.Triangles(compositeOperation), default, default, renderer) { }

        public override void Run(Frame frame, CommandList cmd)
        {

            Pipeline pipeline = PipelineCache.GetPipeLine(renderPipeline, renderer);
            cmd.SetPipeline(pipeline);
            cmd.SetFramebuffer(renderer._device.SwapchainFramebuffer);

            renderer.Shader.SetUniforms(frame, out ResourceSet descriptorSet, uniformOffset, image);
            cmd.SetGraphicsResourceSet(0, descriptorSet);

            cmd.Draw(triangleCount, 1, triangleOffset, 0);
        }

    }
}