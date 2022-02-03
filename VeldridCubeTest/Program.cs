using System.Numerics;
using Engine;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Windowing;
using ObjDemo;
using Veldrid;
using Texture = Engine.Rendering.Texture;

namespace VeldridCubeTest
{
    class CubeTest
    {
        static void Main()
        {
	        Camera fpCam = new Camera(new Vector3(0, 0, 0), -Vector3.UnitZ, Vector3.UnitX , 1.333333F, true );
            Engine.Initialization.Init.InitEngine(0,0, 1024, 768, "Demo Cube", new CubeDemo());
        }
    }

    class CubeDemo : Game
    {
	    Material material;
        public override void Gamestart()
        {
	        new Player();
	        
	        Camera.MainCamera = new Camera(new Vector3(10, 1, 1), -Vector3.UnitZ, Vector3.UnitY,1024/768, true );
	        MaterialDescription materialDescription = new MaterialDescription
			{
				BlendState = BlendStateDescription.SingleOverrideBlend,
				ComparisonKind = ComparisonKind.LessEqual,
				CullMode = FaceCullMode.Back,
				Topology = PrimitiveTopology.TriangleList,
				DepthTest = true,
				WriteDepthBuffer = true,
				FaceDir = FrontFace.CounterClockwise,
				FillMode = PolygonFillMode.Solid,
				Shaders = new Dictionary<ShaderStages, Engine.Rendering.Shader>
				{
					{
						ShaderStages.Fragment,
						new Engine.Rendering.Shader("./Assets/frag.spv", WindowClass._renderer.Device, ShaderStages.Fragment)
					},
					{
						ShaderStages.Vertex,
						new Engine.Rendering.Shader("./Assets/vert.spv", WindowClass._renderer.Device, ShaderStages.Vertex)
					}


				}
			};

			ResourceLayoutDescription vertexLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));

	        Texture atlas = new(WindowClass._renderer.Device, @"Assets\TextureAtlas.tga");
			material = new Material(materialDescription, 
				new VertexLayoutDescription(
					new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
					new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)), 
				WindowClass._renderer.Device.ResourceFactory.CreateResourceLayout(vertexLayout),
				WindowClass._renderer.Device.ResourceFactory.CreateResourceLayout(fragmentLayout)

				);
			material.Sets[0] = WindowClass._renderer.Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(material.layouts[0], WindowClass._renderer.ProjectionBuffer, WindowClass._renderer.ViewBuffer));	
			material.Sets[1] = WindowClass._renderer.Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(material.layouts[1],WindowClass._renderer.WorldBuffer, WindowClass._renderer.Device.Aniso4xSampler, atlas._texture));




			MeshData data = new MeshData()
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
			var Object = new Entity(Engine.MathLib.DoublePrecision_Numerics.Vector3.One, Vector2.Zero);
			Mesh mesh = new Mesh(Object, material);
			mesh.GenerateMesh(ref data);

        }
        
    }    
}

