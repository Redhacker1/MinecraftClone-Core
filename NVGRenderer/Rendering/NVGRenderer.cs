using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NVGRenderer.Rendering.Calls;
using NVGRenderer.Rendering.Pipelines;
using NVGRenderer.Rendering.Shaders;
using NVGRenderer.Rendering.Textures;
using Silk.NET.Maths;
using SilkyNvg;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using SilkyNvg.Rendering;
using SilkyNvg.Rendering.Vulkan;
using Veldrid;
using Path = SilkyNvg.Rendering.Path;
using Texture = SilkyNvg.Rendering.Texture;

namespace NVGRenderer.Rendering;

public class NvgRenderer : INvgRenderer
{
    private readonly RenderFlags _flags;
    public GraphicsDevice _device;
    public TextureManager TextureManager { get; private set; }
    public int DummyTex { get; set; }

    internal NVGShader Shader { get; private set; }

    public bool EdgeAntiAlias => _flags.HasFlag(RenderFlags.Antialias);
    
    internal bool StencilStrokes => _flags.HasFlag(RenderFlags.StencilStrokes);
    
    private VertexCollection _vertexCollection;
    internal bool TriangleListFill => true;

    CallQueue _queue = new CallQueue();
    readonly Frame[] _frames;
    Vector2D<float> _viewSize;
    public uint CurrentFrameIndex { private get; set; }
    public CommandList CurrentCommandBuffer { get; set; }

    internal NvgRendererParams Params { get; }


    public NvgRenderer(NvgRendererParams @params, RenderFlags flags)
    {
        _flags = flags;
        Params = @params;
        _device = Params.Device;
        _frames = new Frame[Params.FrameCount];
        CurrentFrameIndex = 0;
        CurrentCommandBuffer = Params.InitialCommandBuffer;
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
    }

    public bool Create()
    {
        
        PipelineCache.SetDevice(_device);
        
        TextureManager = new TextureManager(this);
        _vertexCollection = new VertexCollection();
        Shader = new NVGShader("SilkyNvg-Vulkan-Shader", EdgeAntiAlias, this);
        
        if (!Shader.Status)
        {
            return false;
        }
        Shader.CreateLayout();
        for (int i = 0; i < _frames.Length; i++)
        {
            _frames[i] = new Frame(this);
        }
        Shader.InitializeFragUniformBuffers();

        Span<Vector4> empty = stackalloc Vector4[] {new Vector4(0)};
        DummyTex = CreateTexture(Texture.Alpha, new Vector2D<uint>(1, 1), 0, MemoryMarshal.Cast<Vector4, byte>(empty));

        
        return true;
    }

    public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
    {
        Engine.Rendering.Veldrid.Texture texture;
        if (data.IsEmpty)
        {
            texture = new Engine.Rendering.Veldrid.Texture(size.X, size.Y, 1, _device);
        }
        else
        {
            texture = Engine.Rendering.Veldrid.Texture.CreateFromBytes(_device, size.X, size.Y, data);   
        }
        TextureSlot slot = new TextureSlot()
        {
            _flags = imageFlags,
            _texture = texture
        };
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
        TextureManager.FindTexture(image, out _)._texture.UpdateTextureBytes(_device, data, bounds.Size.X, bounds.Size.Y);
        return true;
    }

    public bool GetTextureSize(int image, out Vector2D<uint> size)
    {
        TextureSlot imageslot = TextureManager.FindTexture(image, out _);
        size = new Vector2D<uint>(imageslot._texture._Texture.Width, imageslot._texture._Texture.Height);
        return true;
    }

    public void Viewport(Vector2D<float> size, float devicePixelRatio)
    {
        _viewSize = size;
    }

    public void Cancel()
    {
        _vertexCollection.Clear();
        _queue.Clear();
        Shader.UniformManager.Clear();
    }

    public void Flush()
    {
        CurrentCommandBuffer.Begin();
        
        if (_queue.HasCalls)
        {
            Frame frame = _frames[CurrentFrameIndex];

            frame.VertexBuffer.ModifyBuffer(_vertexCollection.Vertices, _device);

            VertUniforms vertUniforms = new VertUniforms
            {
                ViewSize = _viewSize
            };
            
            frame.VertexUniformBuffer.ModifyBuffer(vertUniforms, _device);
            
            CurrentCommandBuffer.SetViewport(0, new Viewport(0,0, _viewSize.X, _viewSize.Y, 0, float.MaxValue));
            //CurrentCommandBuffer.SetScissorRect(0, 0,0, (uint)_viewSize.X, (uint)_viewSize.Y);

            //frame.DescriptorSetManager.Reset(_requireredDescriptorSetCount);
            frame.FragmentUniformBuffer.ModifyBuffer(Shader.UniformManager.Uniforms, _device);
            CurrentCommandBuffer.SetVertexBuffer(0, frame.VertexBuffer.BufferObject);

            _queue.Run(frame, CurrentCommandBuffer);
        }
        CurrentCommandBuffer.End();
        
        _vertexCollection.Clear();
        _queue.Clear();
        Shader.UniformManager.Clear();
        
        //_requireredDescriptorSetCount = 0;
        if (Params.AdvanceFrameIndexAutomatically)
        {
            AdvanceFrame();
        }
    }

    public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Box2D<float> bounds, IReadOnlyList<Path> paths)
    {

        int offset = _vertexCollection.CurrentsOffset;
        StrokePath[] renderPaths = new StrokePath[paths.Count];
        for (int i = 0; i < paths.Count; i++)
        {
            Path path = paths[i];
            int verticesPresentCount = _vertexCollection.CurrentsOffset;
            int fillCount;
            if (TriangleListFill)
            {
                for (int j = 0; j < path.Fill.Count - 2; j++)
                {
                    _vertexCollection.AddVertex(path.Fill[0]);
                    _vertexCollection.AddVertex(path.Fill[j + 1]);
                    _vertexCollection.AddVertex(path.Fill[j + 2]);
                    offset += 3;
                }
                fillCount = (path.Fill.Count - 2) * 3;
            }
            else
            {
                _vertexCollection.AddVertices(path.Fill);
                fillCount = path.Fill.Count;
                offset += fillCount;
            }
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
            //_requireredDescriptorSetCount++;

            ulong uniformOffset = Shader.UniformManager.AddUniform(uniforms);
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
            ulong uniformOffset = Shader.UniformManager.AddUniform(stencilUniforms);
            _ = Shader.UniformManager.AddUniform(uniforms);

            call = new FillCall(paint.Image, renderPaths, (uint)offset, uniformOffset, compositeOperation, this);
        }

        _queue.Add(call);
    }

    public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Path> paths)
    {
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
            ulong uniformOffset = Shader.UniformManager.AddUniform(uniforms);
            _ = Shader.UniformManager.AddUniform(stencilUniforms);

            call = new StencilStrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
        }
        else
        {
            //_requireredDescriptorSetCount++;

            ulong uniformOffset = Shader.UniformManager.AddUniform(uniforms);
            call = new StrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
        }
        _queue.Add(call);
    }

    public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringe)
    {
        
        uint offset = (uint)_vertexCollection.CurrentsOffset;
        _vertexCollection.AddVertices(vertices);

        //_requireredDescriptorSetCount++;

        FragUniforms uniforms = new FragUniforms(paint, scissor, fringe, this);
        ulong uniformOffset = Shader.UniformManager.AddUniform(uniforms);
        Call call = new TrianglesCall(paint.Image, compositeOperation, offset, (uint)vertices.Count, uniformOffset, this);
        _queue.Add(call);
    }
    
    internal void AdvanceFrame()
    {
        CurrentFrameIndex++;
        if (CurrentFrameIndex >= _frames.Length)
        {
            CurrentFrameIndex = 0;
        }
    }

}