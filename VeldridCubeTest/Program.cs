using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using Veldrid;
using Shader = Engine.Rendering.Veldrid.Shader;
using Texture = Engine.Rendering.Veldrid.Texture;

namespace VeldridCubeTest
{
    class CubeTest
    {
        static void Main()
        {
	        Camera fpCam = new Camera(new Vector3(0, 0, 0), -Vector3.UnitZ, Vector3.UnitX , 1.333333F, true );
            Init.InitEngine(0,0, 1024, 768, "Demo Cube", new CubeDemo());
        }
    }

    class CubeDemo : GameEntry
    {
	    Material material;
	    Entity Object;
	    Player _player;
        public override void Gamestart()
        {
	        _player = new Player();
	        
	        Camera.MainCamera = new Camera(new Vector3(10, 1, 1), Vector3.UnitZ, Vector3.UnitY,1024/768, true );
	        MaterialDescription materialDescription = new MaterialDescription
			{
				BlendState = BlendStateDescription.SingleDisabled,
				ComparisonKind = ComparisonKind.LessEqual,
				CullMode = FaceCullMode.None,
				Topology = PrimitiveTopology.TriangleList,
				DepthTest = true,
				WriteDepthBuffer = true,
				FaceDir = FrontFace.Clockwise,
				FillMode = PolygonFillMode.Solid,
				Shaders = new Dictionary<ShaderStages, Shader>
				{
					{
						ShaderStages.Fragment,
						new Shader("./Assets/frag.spv", WindowClass.Renderer.Device, ShaderStages.Fragment)
					},
					{
						ShaderStages.Vertex,
						new Shader("./Assets/vert.spv", WindowClass.Renderer.Device, ShaderStages.Vertex)
					}


				}
			};

			ResourceLayoutDescription vertexLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));
			
			material = new Material(materialDescription, 
				new VertexLayoutDescription(
					new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
					new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)),
				WindowClass.Renderer,
				vertexLayout,
				fragmentLayout
			);
			
			
			var atlas = new Texture(WindowClass.Renderer.Device, @"Assets\TextureAtlas.tga");
			var pointSampler = new TextureSampler(WindowClass.Renderer.Device.PointSampler);
			
			material.ResourceSet(0, WindowClass.Renderer.ViewProjBuffer);
			material.ResourceSet(1,WindowClass.Renderer.WorldBuffer, pointSampler, atlas);


			MeshData data = new MeshData
			{
				_vertices = new[]
				{
					new Vector3(-0.5f, +0.5f, -0.5f), new Vector3(+0.5f, +0.5f, -0.5f),
					new Vector3(+0.5f, +0.5f, +0.5f), new Vector3(-0.5f, +0.5f, +0.5f),
					new Vector3(-0.5f, -0.5f, +0.5f), new Vector3(+0.5f, -0.5f, +0.5f),
					new Vector3(+0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
					new Vector3(-0.5f, +0.5f, -0.5f), new Vector3(-0.5f, +0.5f, +0.5f),
					new Vector3(-0.5f, -0.5f, +0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
					new Vector3(+0.5f, +0.5f, +0.5f), new Vector3(+0.5f, +0.5f, -0.5f),
					new Vector3(+0.5f, -0.5f, -0.5f), new Vector3(+0.5f, -0.5f, +0.5f),
					new Vector3(+0.5f, +0.5f, -0.5f), new Vector3(-0.5f, +0.5f, -0.5f),
					new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(+0.5f, -0.5f, -0.5f),
					new Vector3(-0.5f, +0.5f, +0.5f), new Vector3(+0.5f, +0.5f, +0.5f),
					new Vector3(+0.5f, -0.5f, +0.5f), new Vector3(-0.5f, -0.5f, +0.5f)
				},
				_uvs = new []
				{
					new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0),
					new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0),
					new Vector3(1, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 0),
					new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
					new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0),
					new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0),
					new Vector3(1, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 0),
					new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0)
				},
				_indices = new uint[] 
				{
					0,1,2, 0,2,3, 
					4,5,6, 4,6,7, 
					8,9,10, 8,10,11, 
					12,13,14, 12,14,15, 
					16,17,18, 16,18,19, 
					20,21,22, 20,22,23,
				}
			};
			Object = new Entity(Vector3.Zero, Quaternion.Identity);
			Mesh mesh = new Mesh();
			mesh.GenerateMesh(data);


        }
        
    }    
}

