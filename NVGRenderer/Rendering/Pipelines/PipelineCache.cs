using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace NVGRenderer.Rendering.Pipelines;

public class PipelineCache
{

    NvgFrame _frame;
    public PipelineCache(NvgFrame frame)
    {
        _frame = frame;
    }

    public void SetFrame(NvgFrame frame)
    {
        _frame = frame;
        _pipelines.Clear();
    }


    readonly Dictionary<PipelineSettings, Pipeline> _pipelines = new Dictionary<PipelineSettings, Pipeline>();

    
    public Pipeline GetPipeLine(PipelineSettings renderPipeline, NvgRenderer renderer)
    {
        return !_pipelines.ContainsKey(renderPipeline) ? AddPipeline(renderPipeline, renderer) : _pipelines[renderPipeline];
    }

    public Pipeline AddPipeline(PipelineSettings settings, NvgRenderer renderer)
    {
        DepthStencilStateDescription depthStencil = new DepthStencilStateDescription(settings.DepthTestEnabled, settings.DepthTestEnabled, ComparisonKind.LessEqual,
            settings.StencilTestEnable, new StencilBehaviorDescription(settings.FrontStencilFailOp,
                settings.FrontStencilPassOp, settings.FrontStencilDepthFailOp, settings.StencilFunc
            ),
            new StencilBehaviorDescription(settings.BackStencilFailOp, settings.BackStencilPassOp,
                settings.BackStencilDepthFailOp, settings.StencilFunc), (byte) settings.StencilMask,
            (byte) settings.StencilMask, settings.StencilRef
        );

        RasterizerStateDescription rasterizerState = new RasterizerStateDescription(settings.CullMode,
            PolygonFillMode.Solid, settings.FrontFace, false, true);

        BlendAttachmentDescription blendAttachment = new BlendAttachmentDescription
        {
            ColorWriteMask = settings.ColourMask
        };

        Shader vs;
        Shader fs;

        VertexElementDescription[] vertexElementDescription;
        
        GraphicsDevice device = _frame.Renderer.Device;
        switch (device.BackendType)
        {
            case GraphicsBackend.Direct3D11:
            {
                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(),
                    renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), CrossCompileTarget.HLSL);
                vs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(things.VertexShader), "main"));
                fs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(things.FragmentShader), "main"));
                vertexElementDescription = things.Reflection.VertexElements;
                break;
            }
            case GraphicsBackend.OpenGL:
            {
                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(),
                    renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), CrossCompileTarget.GLSL);
                vs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(things.VertexShader), "main"));
                fs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(things.FragmentShader), "main"));
                vertexElementDescription = things.Reflection.VertexElements;
                break;
            }
            case GraphicsBackend.OpenGLES:
            {
                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(),
                    renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), CrossCompileTarget.ESSL);
                vs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(things.VertexShader), "main"));
                fs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(things.FragmentShader), "main"));
                vertexElementDescription = things.Reflection.VertexElements;
                break;
            }
            case GraphicsBackend.Metal:
            {
                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(),
                    renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), CrossCompileTarget.MSL);
                vs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(things.VertexShader), "main"));
                fs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(things.FragmentShader), "main"));
                vertexElementDescription = things.Reflection.VertexElements;
                break;
            }
            case GraphicsBackend.Vulkan:
                vs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex ,renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(), "main"));
                fs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment ,renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), "main"));
                vertexElementDescription = new[] {
                    new VertexElementDescription("vertex",  VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                    new VertexElementDescription("tcoord",  VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ShaderSetDescription shaderSet = new ShaderSetDescription(new[]
        {
            new VertexLayoutDescription(vertexElementDescription)
        }, new[]{vs, fs});

        BlendStateDescription blendState = BlendStateDescription.SingleDisabled;
        blendState.AttachmentStates = new[] {blendAttachment};

        Pipeline pipeline = device.ResourceFactory.CreateGraphicsPipeline(
            new GraphicsPipelineDescription(blendState, depthStencil, rasterizerState, settings.Topology, shaderSet, new []{renderer.DescriptorSetLayout}, device.MainSwapchain.Framebuffer.OutputDescription, ResourceBindingModel.Default));

        _pipelines[settings] = pipeline;

        return pipeline;
    }

    public void Dispose()
    {
        foreach (KeyValuePair<PipelineSettings, Pipeline> pipelineSet in _pipelines)
        {
            pipelineSet.Value.Dispose();
        }
    }
}