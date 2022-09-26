using System.Numerics;
using System.Runtime.CompilerServices;
using Engine;
using Engine.Initialization;
using Engine.MathLib;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using Veldrid;
using Shader = Engine.Rendering.Abstract.Shader;
using Texture = Engine.Rendering.Veldrid.Texture;

namespace VeldridCubeTest
{
    class CubeTest
    {
        static void Main()
        {
	        Init.InitEngine(0,0, 1024, 768, "Demo Cube", new CubeDemo());
        }
    }

    class CubeDemo : GameEntry
    {
	    Instance3D _instance3D;
	    Material material;
	    Entity Object;
	    Player _player;

	    BaseLevel _level = new BaseLevel();

	    protected override void GameStart()
	    {
		    base.GameStart();

		    PinnedObject = _level;
	        _player = new Player();
	        _level.AddChild(_player);
	        _player._Ready();
	        
	        Camera.MainCamera = new Camera(new Transform(), Vector3.UnitZ, Vector3.UnitY,1024/768, true );
	        
	        ShaderSet set = new ShaderSet(
		        ShaderExtensions.CreateShaderFromFile(ShaderType.Vertex, "./Assets/shader.vert", "main", ShaderExtensions.ShadingLanguage.GLSL),
		        ShaderExtensions.CreateShaderFromFile(ShaderType.Fragment, "./Assets/shader.frag", "main", ShaderExtensions.ShadingLanguage.GLSL));
	        
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
				Shaders = set,
			};

			ResourceLayoutDescription vertexLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));
			
			material = new Material(materialDescription, 
				new VertexLayoutDescription[2]
				{
					new VertexLayoutDescription((uint)Unsafe.SizeOf<Matrix4x4>(), 1, 
						new VertexElementDescription("Matrix1xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
						new VertexElementDescription("Matrix2xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
						new VertexElementDescription("Matrix3xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
						new VertexElementDescription("Matrix4xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)),
					
					new VertexLayoutDescription(
						new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
						new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
				},
				WindowClass.Renderer,
				new []{				
					vertexLayout,
					fragmentLayout
					
				}
			);
			
			
			var atlas = new Texture(WindowClass.Renderer.Device, @"Assets\TextureAtlas.tga");
			var pointSampler = new TextureSampler(WindowClass.Renderer.Device.PointSampler);
			
			material.ResourceSet(1, pointSampler, atlas);


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
			Object = new Entity(new Transform());
			Mesh mesh = new Mesh();
			mesh.GenerateMesh(data);

			_instance3D = new Instance3D(mesh, material);
			_instance3D.SetTransform(new Transform());
			
			_level.AddChild(_instance3D);
			
			WindowClass.Renderer.Passes[0].AddInstance(_instance3D);


        }
        
    }    
}

