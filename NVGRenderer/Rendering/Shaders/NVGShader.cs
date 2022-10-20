using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Engine.Rendering.Abstract;

namespace NVGRenderer.Rendering.Shaders;

internal class NVGShader
{

    private readonly Shader _vertexShaderModule, _fragmentShaderModule;

    private readonly string _name;
    private readonly NvgRenderer _renderer;


    private ulong _align;

    public Shader VertexShaderStage => _vertexShaderModule;

    public Shader FragmentShaderStage => _fragmentShaderModule;

    public ulong FragSize { get; private set; }
    

    public bool Status { get; private set; } = true;

    public NVGShader(string name, bool edgeAA, NvgRenderer renderer)
    {
        _renderer = renderer;
        _name = name;

        _vertexShaderModule = CreateShader(
            ShaderExtensions.CreateShaderFromText(
                Engine.Rendering.Abstract.ShaderType.Vertex,
                ShaderCode.VertexShaderCode,
                "NVGVertexShader",
                "main",
                ShaderExtensions.ShadingLanguage.GLSL).Shaderbytes.ToArray()
            , Engine.Rendering.Abstract.ShaderType.Vertex );

        string isAAString = edgeAA ? "AA" : "NonAA";
        _fragmentShaderModule = CreateShader( 
            ShaderExtensions.CreateShaderFromText(Engine.Rendering.Abstract.ShaderType.Fragment, edgeAA ? ShaderCode.FragmentShaderEdgeAaCode : ShaderCode.FragmentShaderCode,
                $"NVGFragmentShader{isAAString}",
                "main",
                ShaderExtensions.ShadingLanguage.GLSL).Shaderbytes.ToArray(),
            Engine.Rendering.Abstract.ShaderType.Fragment);
    }

    static Shader CreateShader(byte[] code, Engine.Rendering.Abstract.ShaderType type)
    {
        return new Shader(code.ToImmutableArray(), type, "main");
    }

    public void InitializeFragUniformBuffers()
    {
        _align = _renderer.Device.UniformBufferMinOffsetAlignment;

        FragSize = ((ulong)Marshal.SizeOf(typeof(FragUniforms))) + _align - (((ulong)Marshal.SizeOf(typeof(FragUniforms))) % _align);
        
    }

}