using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Engine.Rendering.Veldrid;
using NVGRenderer.Rendering.Calls;
using NVGRenderer.Rendering.Pipelines;
using NVGRenderer.Rendering.Shaders;
using Silk.NET.Maths;
using SilkyNvg.Rendering;
using Veldrid;

namespace NVGRenderer.Rendering;

public class NvgFrame : IDisposable
{

    public readonly NvgRenderer Renderer;

    public VertexBuffer<Vertex> VertexBuffer { get; }

    public UniformBuffer<VertUniforms> VertexUniformBuffer { get; }

    public readonly ResourceSetCache ResourceSetCache;

    public readonly PipelineCache PipelineCache;

    public readonly UniformManager UniformAllocator;

    public Framebuffer Framebuffer;

    public VertexBuffer<byte> FragmentUniformBuffer { get; }
    public readonly CallQueue Queue;

    public Swapchain Swapchain;

    public Vector2D<uint> Size;





    public NvgFrame(NvgRenderer renderer, NvgFrameBufferParams parameters)
    {
        
        ResourceSetCache = new ResourceSetCache(this);

        uint alignment = renderer.Device.StructuredBufferMinOffsetAlignment;
        uint fragSize = (uint) (Unsafe.SizeOf<FragUniforms>());
        UniformAllocator = new UniformManager(fragSize);

        Queue = new CallQueue(this);
        
        PipelineCache = new PipelineCache(this);
        
        Renderer = renderer;

        VertexBuffer = new VertexBuffer<Vertex>(Renderer.Device, new Vertex[1]);

        VertexUniformBuffer = new UniformBuffer<VertUniforms>(Renderer.Device, 1u);

        // This is an ugly hack, I will need to fix it later
        FragmentUniformBuffer = new VertexBuffer<byte>(Renderer.Device, new byte[32]);

        Framebuffer = parameters.Framebuffer;
        
        renderer.SetFrame(this);
            
    }

    public void Dispose()
    {
        
        VertexBuffer.Dispose();
        VertexUniformBuffer.Dispose();
        FragmentUniformBuffer.Dispose();
        PipelineCache.Clear();
        ResourceSetCache.Clear();
    }

    public void Clear()
    {
        // This should not be needed, but it is alas :(
        ResourceSetCache.Clear();
        UniformAllocator.Clear();
        Queue.Clear();
    }
}