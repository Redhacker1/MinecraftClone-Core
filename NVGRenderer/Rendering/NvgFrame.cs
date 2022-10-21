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

    public StructuredBuffer<FragUniforms> FragmentUniformBuffer { get; }
    public readonly CallQueue Queue;

    public Swapchain Swapchain;

    public Vector2D<uint> Size;





    public NvgFrame(NvgRenderer renderer, NvgFrameBufferParams parameters)
    {
        
        ResourceSetCache = new ResourceSetCache(this);

        uint alignment = renderer.Device.StructuredBufferMinOffsetAlignment;
        uint fragSize = (uint)(Unsafe.SizeOf<FragUniforms>() + alignment - (Unsafe.SizeOf<FragUniforms>() % alignment));
        UniformAllocator = new UniformManager(fragSize);

        Queue = new CallQueue(this);
        
        PipelineCache = new PipelineCache(this);
        
        Renderer = renderer;

        VertexBuffer = new VertexBuffer<Vertex>(Renderer.Device, new Vertex[1]);

        VertexUniformBuffer = new UniformBuffer<VertUniforms>(Renderer.Device, 1u);

        FragmentUniformBuffer = new StructuredBuffer<FragUniforms>(Renderer.Device, 1u, alignment, false);
        renderer.SetFrame(this);
            
    }

    public void Dispose()
    {
        VertexBuffer.Dispose();
        VertexUniformBuffer.Dispose();
        FragmentUniformBuffer.Dispose();
    }

    public void Clear()
    {
        UniformAllocator.Clear();
        Queue.Clear();
    }
}