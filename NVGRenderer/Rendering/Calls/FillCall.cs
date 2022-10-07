using NVGRenderer.Rendering.Pipelines;
using SilkyNvg.Blending;
using Veldrid;


namespace NVGRenderer.Rendering.Calls
{
    internal class FillCall : Call
    {

        public FillCall(int image, StrokePath[] paths, uint triangleOffset, ulong uniformOffset, CompositeOperationState compositeOperation, NvgRenderer renderer)
            : base(image, paths, triangleOffset, 4, uniformOffset, PipelineSettings.Fill(compositeOperation), PipelineSettings.FillStencil(compositeOperation, renderer.TriangleListFill), PipelineSettings.FillEdgeAa(compositeOperation), renderer) { }

        public override void Run(Frame frame, CommandList cmd)
        {

            Pipeline sPipeline = PipelineCache.GetPipeLine(stencilPipeline, renderer); 
            cmd.SetPipeline(sPipeline);
            
            renderer.Shader.SetUniforms(frame, out ResourceSet descriptorSet, uniformOffset, 0);
            cmd.SetGraphicsResourceSet(0, descriptorSet);
            foreach (StrokePath path in paths)
            {
                cmd.Draw(path.FillCount, 1, path.FillOffset, 0);
            }


            renderer.Shader.SetUniforms(frame, out descriptorSet, uniformOffset + renderer.Shader.FragSize, image);
            cmd.SetGraphicsResourceSet(0, descriptorSet);

            if (renderer.EdgeAntiAlias)
            {
                Pipeline aaPipeline = PipelineCache.GetPipeLine(antiAliasPipeline, renderer);
                cmd.SetPipeline(aaPipeline);
                foreach (StrokePath path in paths)
                {
                    cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                }
            }

            Pipeline fPipeline = PipelineCache.GetPipeLine(renderPipeline, renderer); 
            cmd.SetPipeline(fPipeline);
            cmd.Draw(triangleCount, 1, triangleOffset, 0);
        }

    }
}