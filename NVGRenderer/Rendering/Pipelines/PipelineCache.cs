using System.Text;
using Engine.Rendering.Abstract;
using NVGRenderer.Rendering.Shaders;
using SilkyNvg.Rendering;
using Veldrid;
using Veldrid.SPIRV;
using Shader = Veldrid.Shader;

namespace NVGRenderer.Rendering.Pipelines;

public class PipelineCache
{
    static GraphicsDevice _device;
    
    
    
    static Dictionary<PipelineSettings, Pipeline> _pipelines = new Dictionary<PipelineSettings, Pipeline>();


    public static void SetDevice(GraphicsDevice device)
    {
        _device = device;
    }
    public static Pipeline GetPipeLine(PipelineSettings renderPipeline, NvgRenderer renderer)
    {
        return !_pipelines.ContainsKey(renderPipeline) ? AddPipeline(renderPipeline, renderer) : _pipelines[renderPipeline];
    }

    public static Pipeline AddPipeline(PipelineSettings settings, NvgRenderer renderer)
    {
        DepthStencilStateDescription depthStencil = new DepthStencilStateDescription(settings.DepthTestEnabled, settings.DepthWrite,
            settings.DepthWriteMode,
            settings.StencilTestEnable, new StencilBehaviorDescription(settings.FrontStencilFailOp,
                settings.FrontStencilPassOp, settings.FrontStencilDepthFailOp, settings.StencilFunc
            ),
            new StencilBehaviorDescription(settings.BackStencilFailOp, settings.BackStencilPassOp,
                settings.BackStencilDepthFailOp, settings.StencilFunc), (byte) settings.StencilMask,
            (byte) settings.StencilMask, settings.StencilRef
        );

        RasterizerStateDescription rasterizerState = new RasterizerStateDescription(settings.CullMode,
            PolygonFillMode.Solid, settings.FrontFace, false, true);

        BlendAttachmentDescription blendAttachment = new BlendAttachmentDescription()
        {
            ColorWriteMask = settings.ColourMask
        };

        Pipeline pipeline;

        Shader vs;
        Shader fs;

        VertexElementDescription[] vertexElementDescription;
        
        switch (_device.BackendType)
        {
            case GraphicsBackend.Direct3D11:
            {
                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(),
                    renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), CrossCompileTarget.HLSL);
                vs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(things.VertexShader), "main"));
                fs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(things.FragmentShader), "main"));
                vertexElementDescription = things.Reflection.VertexElements;
                break;
            }
            case GraphicsBackend.OpenGL:
            {
                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(),
                    renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), CrossCompileTarget.GLSL);
                vs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(things.VertexShader), "main"));
                fs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(things.FragmentShader), "main"));
                vertexElementDescription = things.Reflection.VertexElements;
                break;
            }
            case GraphicsBackend.OpenGLES:
            {
                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(),
                    renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), CrossCompileTarget.ESSL);
                vs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(things.VertexShader), "main"));
                fs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(things.FragmentShader), "main"));
                vertexElementDescription = things.Reflection.VertexElements;
                break;
            }
            case GraphicsBackend.Metal:
            {
                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(),
                    renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), CrossCompileTarget.MSL);
                vs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(things.VertexShader), "main"));
                fs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(things.FragmentShader), "main"));
                vertexElementDescription = things.Reflection.VertexElements;
                break;
            }
            case GraphicsBackend.Vulkan:
                vs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex ,renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(), "main"));
                fs = _device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex ,renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), "main"));
                vertexElementDescription = new[] { new VertexElementDescription("data",  VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate)};
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ShaderSetDescription shaderSet = new ShaderSetDescription(new VertexLayoutDescription[]
        {
            new VertexLayoutDescription(vertexElementDescription)
        }, new[]{vs, fs});

        BlendStateDescription blendState = BlendStateDescription.SingleDisabled;
        blendState.AttachmentStates = new[] {blendAttachment};

        pipeline = _device.ResourceFactory.CreateGraphicsPipeline(
            new GraphicsPipelineDescription(blendState, depthStencil, rasterizerState, settings.Topology, shaderSet, new []{renderer.Shader.DescriptorSetLayout}, _device.MainSwapchain.Framebuffer.OutputDescription, ResourceBindingModel.Default));

        return pipeline;
    }
}