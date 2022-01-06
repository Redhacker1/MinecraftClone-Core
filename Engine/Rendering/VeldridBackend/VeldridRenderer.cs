using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Engine.Renderable;
using Engine.Rendering.Shared;
using Engine.Rendering.Shared.Buffers;
using Engine.Rendering.Shared.Pipeline;
using Engine.Rendering.Shared.Shaders;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using Veldrid;
using Veldrid.SPIRV;
using Shader = Engine.Rendering.Shared.Shaders.Shader;

namespace Engine.Rendering.VeldridBackend
{
    sealed class VeldridRenderer : BaseRenderer
    {
        bool loaded = false;
        ResourceFactory _factory;
        GraphicsDevice _device;
        CommandList _list;
        
        /// Temporary test code to ensure veldrid is working
        private static DeviceBuffer _vertexBuffer;
        private static DeviceBuffer _indexBuffer;
        private static Veldrid.Shader[] _shaders;
        private static Pipeline _veldridBackendPipeline;
        
        private const string VertexCode = @"
// empty shader file
#version 450
layout(location = 0) in vec2 Position;
layout(location = 1) in uint Color;

layout(location = 0) out vec4 fsin_Color;

void main()
	{
	vec4 color = vec4(Color & 16711680, Color & 16711680, Color & 65280, Color & 255);
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = color;
    }";

        private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";
        
        
        
        
        internal VeldridRenderer(IView view) : base(view)
        {
        }

        internal override BufferSet CreateBufferSet(IReadOnlyCollection<GenericBuffer> buffers, uint size)
        {
            throw new System.NotImplementedException();
            return null;
        }

        internal override BaseBufferObject<T>CreateBuffer<T>(BufferType bufferType, T[] data)
        {
            return new VeldridBuffer<T>(data, bufferType, _device);
        }

        public override Shader CreateShader(string shaderPath, ShaderType stage)
        {
            throw new NotImplementedException();
            return null;
        }

        public override Shared.Texture CreateTexture(string texturePath)
        {
            throw new NotImplementedException();
            return null;
        }

        internal override GraphicsRenderState CreateRenderState(RenderStateDescription description)
        {
            return new VeldridBackendPipeline(description, _device);
        }


        internal override void OnResized(Vector2D<int> size)
        {
            _device.MainSwapchain.Resize((uint)size.X, (uint)size.Y);
        }

        private void CreateResources()
        {

            ResourceFactory factory = _device.ResourceFactory;

            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector2(-.75f, .75f), RgbaFloat.Red),
                new VertexPositionColor(new Vector2(.75f, .75f), RgbaFloat.Green),
                new VertexPositionColor(new Vector2(-.75f, -.75f), RgbaFloat.Blue),
                new VertexPositionColor(new Vector2(.75f, -.75f), RgbaFloat.Yellow)
            };
            BufferDescription vbDescription = new BufferDescription(
                4 * VertexPositionColor.SizeInBytes,
                BufferUsage.VertexBuffer);
            _vertexBuffer = factory.CreateBuffer(vbDescription);
            _device.UpdateBuffer(_vertexBuffer, 0, quadVertices);

            ushort[] quadIndices = { 0, 1, 2, 3 };
            BufferDescription ibDescription = new BufferDescription(
                4 * sizeof(ushort),
                BufferUsage.IndexBuffer);
            _indexBuffer = factory.CreateBuffer(ibDescription);
            _device.UpdateBuffer(_indexBuffer, 0, quadIndices);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(VertexCode),
                "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(FragmentCode),
                "main");

            _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            // Create pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: _shaders);
            pipelineDescription.Outputs = _device.SwapchainFramebuffer.OutputDescription;

            _veldridBackendPipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            _list = factory.CreateCommandList();
        }

        void SetMeshReferenceActive(BufferSet Data, CommandList _list)
        {
            foreach (var VARIABLE in )
            {
                
            }
        }
        
        

        internal override void OnRender(double time, IEnumerable<MeshInstance> scene)
        {
            // Begin() must be called before commands can be issued.
            _list.Begin();
            _list.ClearDepthStencil(1f);
            foreach (MeshInstance mesh in scene)
            {
                _list.SetPipeline((mesh.renderstate as VeldridBackendPipeline)?.backingpipeline);
                SetMeshReferenceActive(mesh.Mesh.MeshReference, _list);
            }
            
            
            _list.End();
            _device.SubmitCommands(_list);
            
            _device.WaitForIdle();
            // Once commands have been submitted, the rendered image can be presented to the application window.
            _device.SwapBuffers();
        }

        internal override void OnLoad()
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true
            };
            
            _device =  _windowHandle.CreateGraphicsDevice(options, GraphicsBackend.Vulkan);
            _factory = _device.ResourceFactory;
            _list = _factory.CreateCommandList();
            CreateResources();
        }
    }
    struct VertexPositionColor
    {
        public const uint SizeInBytes = 24;
        public Vector2 Position;
        public RgbaFloat Color;
        public VertexPositionColor(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
    }
    
}