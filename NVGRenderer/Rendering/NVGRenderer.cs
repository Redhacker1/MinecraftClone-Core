using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Engine.Utilities.LowLevel.EnumLib;
using Engine.Windowing;
using NVGRenderer.Rendering.Calls;
using NVGRenderer.Rendering.Draw;
using NVGRenderer.Rendering.Shaders;
using NVGRenderer.Rendering.Textures;
using Silk.NET.Maths;
using SilkyNvg;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using SilkyNvg.Rendering;
using Veldrid;
using Path = SilkyNvg.Rendering.Path;
using Texture = SilkyNvg.Rendering.Texture;

namespace NVGRenderer.Rendering;

public class NvgRenderer : INvgRenderer, IDisposable
{
    private readonly RenderFlags _flags;
    public readonly GraphicsDevice Device;
    public TextureManager TextureManager { get; private set; }
    public int DummyTex { get; set; }

    internal NVGShader Shader { get; private set; }

    public bool EdgeAntiAlias => FastFlagCompare.HasFlag((int)_flags, (int)RenderFlags.Antialias);

    internal bool StencilStrokes => FastFlagCompare.HasFlag((int)_flags, (int)RenderFlags.StencilStrokes);

    private readonly VertexCollection _vertexCollection;

    Vector2D<float> _viewSize;
    public uint CurrentFrameIndex { private get; set; }
    public CommandList CurrentCommandBuffer { get; set; }
    internal NvgRendererParams Params { get; }

    NvgFrame _frame;

    public void SetFrame(NvgFrame frame)
    {
        _frame = frame;
    }



    public NvgRenderer(NvgRendererParams @params, RenderFlags flags)
    {
        _flags = flags;
        Params = @params;
        Device = Params.Device;
        CurrentFrameIndex = 0;
        CurrentCommandBuffer = Params.InitialCommandBuffer;

        TextureManager = new TextureManager(this);
        _vertexCollection = new VertexCollection();
        Shader = new NVGShader("SilkyNvg-Vulkan-Shader", EdgeAntiAlias, this);

        Span<Vector4> empty = stackalloc Vector4[] { new Vector4(0) };
        DummyTex = CreateTexture(Texture.Alpha, new Vector2D<uint>(1, 1), 0, MemoryMarshal.Cast<Vector4, byte>(empty));
        Shader.InitializeFragUniformBuffers();

    }

    public void Dispose()
    {
        TextureManager.Dispose();
    }

    public bool Create()
    {

        if (!Shader.Status)
        {
            return false;
        }

        CreateLayout();


        return true;
    }


    public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
    {
        Engine.Rendering.Veldrid.Texture texture = data.IsEmpty ?
            new Engine.Rendering.Veldrid.Texture(size.X, size.Y, 1, Device) :
            Engine.Rendering.Veldrid.Texture.CreateFromBytes(Device, size.X, size.Y, data);

        TextureSlot slot = new TextureSlot
        {
            Flags = imageFlags,
            _Texture = texture
        };
        slot.TextureSampler = (imageFlags & ImageFlags.Nearest) != 0 ? Device.PointSampler : Device.LinearSampler;

        int index = TextureManager.AddTexture(slot);

        return index;
    }

    public bool DeleteTexture(int image)
    {
        TextureManager.DeleteTexture(image);
        return true;
    }

    public bool UpdateTexture(int image, Rectangle<uint> bounds, ReadOnlySpan<byte> data)
    {
        TextureManager.FindTexture(image, out _)._Texture.UpdateTextureBytes(Device, data, bounds.Size.X, bounds.Size.Y);
        return true;
    }

    public bool GetTextureSize(int image, out Vector2D<uint> size)
    {
        TextureSlot imageSlot = TextureManager.FindTexture(image, out _);
        size = new Vector2D<uint>(imageSlot._Texture._Texture.Width, imageSlot._Texture._Texture.Height);
        return true;
    }

    public void Viewport(Vector2D<float> size, float devicePixelRatio)
    {
        _viewSize = size;
    }

    public void Cancel()
    {
        _vertexCollection.Clear();
        AdvanceFrame();
    }

    public void Flush()
    {
        if (_frame == null)
        {
            return;
        }
        
        //Console.WriteLine($"{calls.Count} to run total!");
        CurrentCommandBuffer.Begin();
        CurrentCommandBuffer.SetFramebuffer(WindowClass.Renderer.Device.MainSwapchain.Framebuffer);
        Viewport viewport = new Viewport(0, 0, _viewSize.X, _viewSize.Y, 0, 1);
        CurrentCommandBuffer.SetViewport(0, viewport);
        CurrentCommandBuffer.SetScissorRect(0, 0, 0, (uint)_viewSize.X, (uint)_viewSize.Y);
        
        
        
        _frame.FragmentUniformBuffer.ModifyBuffer(_frame.UniformAllocator.AsSpan(), _frame.UniformAllocator.Alignment);
        _frame.VertexBuffer.ModifyBuffer(_vertexCollection.Vertices, Device);
        
        VertUniforms vertUniforms = new VertUniforms
        {
            ViewSize = _viewSize
        };
        
        _frame.VertexUniformBuffer.ModifyBuffer(vertUniforms);
        
        List<DrawCall> calls = _frame.Queue.CreateDrawCalls();




        //Console.WriteLine($"{_frame.UniformAllocator.Uniforms.Length / _frame.UniformAllocator.FragSize} Uploaded!");

        foreach (DrawCall drawCall in calls)
        {
            //if()
            
            CurrentCommandBuffer.SetPipeline(drawCall.Pipeline);
            CurrentCommandBuffer.SetGraphicsResourceSet(0, _frame.ResourceSetCache.GetResourceSet(drawCall.Set));
            CurrentCommandBuffer.SetVertexBuffer(0, _frame.VertexBuffer.BufferObject);
            CurrentCommandBuffer.Draw(drawCall.Count, 1, drawCall.Offset, 0);
            //Console.WriteLine("Testing");
        }

        CurrentCommandBuffer.End();
        //Device.SubmitCommands(CurrentCommandBuffer);
        _vertexCollection.Clear();


        // Drawcalls go here!

        // Clear frame!
        AdvanceFrame();

    }

    public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Box2D<float> bounds, IReadOnlyList<Path> paths)
    {

        if (_frame == null)
        {
            return;
        }

        int offset = _vertexCollection.CurrentsOffset;
        StrokePath[] renderPaths = new StrokePath[paths.Count];
        for (int i = 0; i < paths.Count; i++)
        {
            Path path = paths[i];
            int verticesPresentCount = _vertexCollection.CurrentsOffset;
            for (int j = 0; j < path.Fill.Count - 2; j++)
            {
                _vertexCollection.AddVertex(path.Fill[0]);
                _vertexCollection.AddVertex(path.Fill[j + 1]);
                _vertexCollection.AddVertex(path.Fill[j + 2]);
                offset += 3;
            }
            int fillCount = (path.Fill.Count - 2) * 3;
            renderPaths[i] = new StrokePath(
                verticesPresentCount, fillCount,
                verticesPresentCount + fillCount, path.Stroke.Count
            );
            _vertexCollection.AddVertices(path.Stroke);
            offset += path.Stroke.Count;
        }

        FragUniforms uniforms = new FragUniforms(paint, scissor, fringe, fringe, -1.0f, this);
        Call call;
        if ((paths.Count == 1) && paths[0].Convex) // Convex
        {
            //_requiredDescriptorSetCount++;
            int uniformOffset = _frame.UniformAllocator.AddUniform(uniforms);
            call = new ConvexFillCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
        }
        else
        {
            //_requireredDescriptorSetCount += 2;

            _vertexCollection.AddVertex(new Vertex(bounds.Max, 0.5f, 1.0f));
            _vertexCollection.AddVertex(new Vertex(bounds.Max.X, bounds.Min.Y, 0.5f, 1.0f));
            _vertexCollection.AddVertex(new Vertex(bounds.Min.X, bounds.Max.Y, 0.5f, 1.0f));
            _vertexCollection.AddVertex(new Vertex(bounds.Min, 0.5f, 1.0f));

            FragUniforms stencilUniforms = new FragUniforms(-1.0f, ShaderType.Simple);
            int uniformOffset = _frame.UniformAllocator.AddUniform(stencilUniforms);
            _ = _frame.UniformAllocator.AddUniform(uniforms);

            call = new FillCall(paint.Image, renderPaths, (uint)offset, uniformOffset, compositeOperation, this);
        }

        _frame.Queue.Add(call);
    }

    public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Path> paths)
    {
        if (_frame == null)
        {
            return;
        }

        int offset = _vertexCollection.CurrentsOffset;
        StrokePath[] renderPaths = new StrokePath[paths.Count];
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].Stroke.Count > 0)
            {
                renderPaths[i] = new StrokePath(0, 0, offset, paths[i].Stroke.Count);
            }
            else
            {
                renderPaths[i] = default;
            }
            _vertexCollection.AddVertices(paths[i].Stroke);
            offset += paths[i].Stroke.Count;
        }

        FragUniforms uniforms = new(paint, scissor, strokeWidth, fringe, -1.0f, this);
        Call call;
        if (StencilStrokes)
        {
            //_requireredDescriptorSetCount += 2;

            FragUniforms stencilUniforms = new(paint, scissor, strokeWidth, fringe, 1.0f - 0.5f / 255.0f, this);

            int uniformOffset = _frame.UniformAllocator.AddUniform(uniforms);
            _ = _frame.UniformAllocator.AddUniform(stencilUniforms);
            call = new StencilStrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
        }
        else
        {
            //_requireredDescriptorSetCount++;
            int uniformOffset = _frame.UniformAllocator.AddUniform(uniforms);
            call = new StrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
        }
        _frame.Queue.Add(call);


    }

    public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringe)
    {
        if (_frame == null)
        {
            return;
        }

        uint offset = (uint)_vertexCollection.CurrentsOffset;
        _vertexCollection.AddVertices(vertices);

        //_requireredDescriptorSetCount++;

        FragUniforms uniforms = new FragUniforms(paint, scissor, fringe, this);
        int uniformOffset = _frame.UniformAllocator.AddUniform(uniforms);
        Call call = new TrianglesCall(paint.Image, compositeOperation, offset, (uint)vertices.Count, uniformOffset, this);
        _frame.Queue.Add(call);
    }

    internal void AdvanceFrame()
    {
        _frame.Clear();
        _frame.ResourceSetCache.Clear();
    }

    public void CreateLayout()
    {
        GraphicsDevice device = Device;

        ResourceLayoutDescription descriptorSetLayoutBindings = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("VertexUniforms", ResourceKind.UniformBuffer, ShaderStages.Vertex,
                ResourceLayoutElementOptions.None),
            new ResourceLayoutElementDescription("FragUniforms", ResourceKind.StructuredBufferReadOnly, ShaderStages.Fragment,
                ResourceLayoutElementOptions.None),
            new ResourceLayoutElementDescription("texsampler", ResourceKind.Sampler, ShaderStages.Fragment,
                ResourceLayoutElementOptions.None),
            new ResourceLayoutElementDescription("tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment,
                ResourceLayoutElementOptions.None)


        );

        ResourceLayout layout = device.ResourceFactory.CreateResourceLayout(descriptorSetLayoutBindings);
        DescriptorSetLayout = layout;
    }

    public ResourceLayout DescriptorSetLayout;
}