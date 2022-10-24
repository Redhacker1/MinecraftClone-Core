using System.Runtime.CompilerServices;
using System.Text;
using NVGRenderer.Rendering.Shaders;
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

        if (!_pipelines.TryGetValue(renderPipeline, out Pipeline potentialPipeline))
        {
            potentialPipeline = AddPipeline(renderPipeline, renderer);
        }
        return potentialPipeline;
    }
    
    public void Clear()
    {
        foreach (KeyValuePair<PipelineSettings, Pipeline> set in _pipelines)
        {
            
            set.Value.Dispose();
        }
        _pipelines.Clear();
    }

    public Pipeline AddPipeline(PipelineSettings settings, NvgRenderer renderer)
    {
        DepthStencilStateDescription depthStencil = DepthStencilState(settings);

        RasterizerStateDescription rasterizerState = RasterizationState(settings);

        Shader vs;
        Shader fs;
        
        
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
                break;
            }
            case GraphicsBackend.Vulkan:
                vs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex ,renderer.Shader.VertexShaderStage.Shaderbytes.ToArray(), "main"));
                fs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment ,renderer.Shader.FragmentShaderStage.Shaderbytes.ToArray(), "main"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ShaderSetDescription shaderSet = new ShaderSetDescription(new[]
        {
            new VertexLayoutDescription(new[]
            {
                new VertexElementDescription("vertex",  VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("tcoord",  VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
            }),
            new VertexLayoutDescription((uint)Unsafe.SizeOf<FragUniforms>(), 1, new []
            {
                //Matrix 1
                new VertexElementDescription("Matrix11xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("Matrix12xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("Matrix13xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                
                //Matrix 2
                new VertexElementDescription("Matrix21xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("Matrix22xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("Matrix23xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                
                new VertexElementDescription("innerCol", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("outerCol", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                
                new VertexElementDescription("scissorExt", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("scissorScale", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("extent", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                
                new VertexElementDescription("rfss", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("tt", VertexElementFormat.Int2, VertexElementSemantic.TextureCoordinate),
                
            }
            )
        }, new[]{vs, fs});

        BlendStateDescription blendState = ColourBlendState(ColorBlendAttachmentState(settings));

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
    
    public static unsafe BlendStateDescription ColourBlendState(BlendAttachmentDescription colourBlendAttachmentState)
    {

        BlendStateDescription Blendstate = new BlendStateDescription();
        Blendstate.AttachmentStates = new[] {colourBlendAttachmentState};
        return Blendstate;
    }
    
    private static BlendAttachmentDescription ColorBlendAttachmentState(PipelineSettings settings)
    {
        return new BlendAttachmentDescription
        {
            ColorWriteMask = settings.ColourMask,
            BlendEnabled= true,
            SourceColorFactor = settings.SrcRgb,
            SourceAlphaFactor = settings.SrcAlpha,
            ColorFunction = BlendFunction.Add,
            DestinationColorFactor = settings.DstRgb,
            DestinationAlphaFactor = settings.DstAlpha,
            AlphaFunction = BlendFunction.Add
        };
    }
    
    private static RasterizerStateDescription RasterizationState(PipelineSettings settings)
    {
        return new()
        {
            FillMode = PolygonFillMode.Solid,
            CullMode = settings.CullMode,
            FrontFace = settings.FrontFace,

            DepthClipEnabled = false,
        };
    }

    
    
    
    public static DepthStencilStateDescription DepthStencilState(PipelineSettings settings)
    {
        return new()
        {
            
            DepthWriteEnabled = false,

            DepthTestEnabled = settings.DepthTestEnabled,
            StencilTestEnabled = settings.StencilTestEnable,

            DepthComparison = ComparisonKind.LessEqual,
            StencilReference = settings.StencilRef,
            StencilWriteMask = settings.StencilWriteMask,
            StencilReadMask = settings.StencilMask,
            

            StencilFront = new()
            {
                Comparison = settings.StencilFunc,
                DepthFail = settings.FrontStencilDepthFailOp,
                Fail = settings.FrontStencilFailOp,
                Pass = settings.FrontStencilPassOp

            },
            StencilBack = new()
            {
                Fail = settings.BackStencilFailOp,
                DepthFail = settings.BackStencilDepthFailOp,
                Pass = settings.BackStencilPassOp,
                Comparison = settings.StencilFunc
            },
        };
    }
}