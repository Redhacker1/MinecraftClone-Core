using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using Veldrid;
using Path = NVGRenderer.Rendering.StrokePath;

namespace NVGRenderer.Rendering.Calls
{
    internal class StrokeCall : Call
    {

        public StrokeCall(int image, Path[] paths, ulong uniformOffset, CompositeOperationState compositeOperation, NvgRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.Stroke(compositeOperation), default, default, renderer) { }

        public override void Run(Frame frame, CommandList cmd)
        {

            Pipeline fillPipeline = PipelineCache.GetPipeLine(renderPipeline, renderer);
            cmd.SetPipeline(fillPipeline);
            cmd.SetFramebuffer(renderer._device.SwapchainFramebuffer);

            renderer.Shader.SetUniforms(frame, out ResourceSet descriptorSet, uniformOffset, image);
            cmd.SetGraphicsResourceSet(0, descriptorSet);
            

            foreach (Path path in paths)
            {
                cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
            }
        }

    }
}