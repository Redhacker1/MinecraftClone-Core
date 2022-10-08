using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Engine.Rendering.Abstract;
using NVGRenderer.Rendering.Textures;
using SharpGen.Runtime;
using Silk.NET.Core.Native;
using Silk.NET.OpenAL;
using Veldrid;
using Shader = Engine.Rendering.Abstract.Shader;

namespace NVGRenderer.Rendering.Shaders;

internal class NVGShader : IDisposable
{

    private readonly Shader _vertexShaderModule, _fragmentShaderModule;

    private readonly string _name;
    private readonly NvgRenderer _renderer;


    private ulong _align;

    public ResourceLayout DescriptorSetLayout { get; private set; }

    public Shader VertexShaderStage => _vertexShaderModule;

    public Shader FragmentShaderStage => _fragmentShaderModule;

    public ulong FragSize { get; private set; }

    public UniformManager UniformManager { get; private set; }

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

    private unsafe Shader CreateShader(byte[] code, Engine.Rendering.Abstract.ShaderType type)
    {
        return new Shader(code.ToImmutableArray(), type, "main");
    }

    public unsafe void CreateLayout()
    {
        GraphicsDevice device = _renderer._device;

        ResourceLayoutDescription descriptorSetLayoutBindings = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("VertexUniforms", ResourceKind.UniformBuffer, ShaderStages.Vertex,
                ResourceLayoutElementOptions.None),
            new ResourceLayoutElementDescription("FragUniforms", ResourceKind.UniformBuffer, ShaderStages.Fragment,
                ResourceLayoutElementOptions.None),
            new ResourceLayoutElementDescription("texsampler", ResourceKind.Sampler, ShaderStages.Fragment,
                ResourceLayoutElementOptions.None),
            new ResourceLayoutElementDescription("tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment,
                ResourceLayoutElementOptions.None)


        );
        
        ResourceLayout layout = device.ResourceFactory.CreateResourceLayout(descriptorSetLayoutBindings);
        DescriptorSetLayout = layout;
    }

    public void InitializeFragUniformBuffers()
    {
        _align = _renderer._device.UniformBufferMinOffsetAlignment;

        FragSize = ((ulong)Marshal.SizeOf(typeof(FragUniforms))) + _align - (((ulong)Marshal.SizeOf(typeof(FragUniforms))) % _align);

        UniformManager = new UniformManager(FragSize);
    }

    public unsafe void SetUniforms(Frame frame, out ResourceSet descriptorSet, ulong uniformOffset, int image)
    {
        int id = _renderer.DummyTex;
        if (image != 0)
        {
            id = image;
        }
        TextureSlot fragmentImageInfo = _renderer.TextureManager.FindTexture(id, out bool _);

        descriptorSet = _renderer._device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
            DescriptorSetLayout,
            frame.VertexUniformBuffer.GetBuffer(),
            new DeviceBufferRange(frame.FragmentUniformBuffer.GetBuffer(), (uint)uniformOffset, frame.FragmentUniformBuffer.Length - (uint)uniformOffset),
            _renderer._device.LinearSampler,
            fragmentImageInfo._texture._Texture
        ));
    }

    public unsafe void Dispose()
    {
        DescriptorSetLayout.Dispose();
    }

}