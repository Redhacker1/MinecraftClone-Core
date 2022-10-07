using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using Veldrid;
using Path = NVGRenderer.Rendering.StrokePath;

namespace NVGRenderer.Rendering.Calls
{
    internal class StencilStrokeCall : Call
    {

        public StencilStrokeCall(int image, Path[] paths, ulong uniformOffset, CompositeOperationState compositeOperation, NvgRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.StencilStroke(compositeOperation), PipelineSettings.StencilStrokeStencil(compositeOperation), PipelineSettings.StencilStrokeEdgeAA(compositeOperation), renderer) { }

        public override void Run(Frame frame, CommandList cmd)
        {


            Pipeline sPipeline = PipelineCache.GetPipeLine(stencilPipeline, renderer);
            cmd.SetPipeline(sPipeline);
            
            
            renderer.Shader.SetUniforms(frame, out ResourceSet descriptorSet, uniformOffset + renderer.Shader.FragSize, image);
            cmd.SetGraphicsResourceSet(0,descriptorSet);
            foreach (Path path in paths)
            {
                cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
            }

            Pipeline aaPipeline = PipelineCache.GetPipeLine(antiAliasPipeline, renderer);
            cmd.SetPipeline(aaPipeline);
            
            renderer.Shader.SetUniforms(frame, out descriptorSet, uniformOffset, image);
            cmd.SetGraphicsResourceSet(0, descriptorSet);
            foreach (Path path in paths)
            {
                cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
            }

            Pipeline stPipeline = PipelineCache.GetPipeLine(renderPipeline, renderer);
            cmd.SetPipeline(stPipeline);
            foreach (Path path in paths)
            {
                cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
            }
        }

    }
}