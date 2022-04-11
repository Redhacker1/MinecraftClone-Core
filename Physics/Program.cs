// See https://aka.ms/new-console-template for more information

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine;
using Engine.Initialization;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Windowing;
using Physics;
using Veldrid;
using Material = Engine.Rendering.Material;
using Texture = Engine.Rendering.Texture;
using Vector3 = System.Numerics.Vector3;

namespace BulletTest
{
    class BulletDemo
    {
        static void Main()
        {
            Init.InitEngine(0,0, 1920, 1080, "BulletDemo", new BulletTest());
        }
    }

    class BulletTest : Game
    {
	    PhysicsManager _physicsManager;
        Cube _cube;
        Floor _floor;
        Material _material;
        Camera cam = new Camera(new Vector3(10, 1, 1), Vector3.UnitZ, Vector3.UnitY,1920/1080, true );
        Player _player = new Player();
        public override void Gamestart()
        {
            base.Gamestart();
            _physicsManager = new PhysicsManager();
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
				new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));
			
			_material = new Material(materialDescription, 
				new VertexLayoutDescription(
					new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
					new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)),
				WindowClass._renderer,
				vertexLayout,
				fragmentLayout
			);
			
			
			var atlas = new Texture(WindowClass._renderer.Device, @"Assets\silk.png");
			var pointSampler = new TextureSampler(WindowClass._renderer.Device.PointSampler);
			
			_material.ResourceSet(0, WindowClass._renderer.ViewProjBuffer);
			_material.ResourceSet(1,WindowClass._renderer.WorldBuffer, pointSampler, atlas);
            
            _cube = new Cube(_material, _physicsManager);
            _floor = new Floor(_material, _physicsManager);
        }
    }



    class PhysicsManager : GameObject
    {
	    
	    public PhysicsManager()
	    {

	        PhysicsTick = true;
        }

        public override void _PhysicsProcess(double delta)
        {
        }

        public void AddPhysicsBody()
        {
        }
    }

    class PhysicsBody
    {
        public PhysicsBody(PhysicsManager CollisionManager)
        {
            
        }
    }

    /// <summary>
    /// Hit the floor
    /// </summary>
    class Floor : GameObject
    {
	    Mesh floor;
	    static MeshData data = new MeshData()
	    {
		    _vertices = new[]
		    {
                    new Vector3(-50f, +10, -50f), new Vector3(+50f, +10f, -50f),
                    new Vector3(+50f, +10f, +50f), new Vector3(-50f, +10f, +50f),
                    new Vector3(-50f, -10f, +50f), new Vector3(+50f, -10f, +50f),
                    new Vector3(+50f, -10f, -50f), new Vector3(-50f, -10f, -50f),
                    new Vector3(-50f, +10f, -50f), new Vector3(-50f, +10f, +50f),
                    new Vector3(-50f, -10f, +50f), new Vector3(-50f, -10f, -50f),
                    new Vector3(+50f, +10f, +50f), new Vector3(+50f, +10f, -50f),
                    new Vector3(+50f, -10f, -50f), new Vector3(+50f, -10f, +50f),
                    new Vector3(+50f, +10f, -50f), new Vector3(-50f, +10f, -50f),
                    new Vector3(-50f, -10f, -50f), new Vector3(+50f, -10f, -50f),
                    new Vector3(-50f, +10f, +50f), new Vector3(+50f, +10f, +50f),
                    new Vector3(+50f, -10f, +50f), new Vector3(-50f, -10f, +50f)
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
	    
	    public Floor(Material material, PhysicsManager physics)
	    {
		    Pos.Y = -100;
		    floor = new Mesh(this, material);
		    floor.GenerateMesh(data);
		    material.AddReference(floor);
		    

	    }
	    
	    public override unsafe void _PhysicsProcess(double delta)
	    {
		    
		    
	    }
    }
    

    /// <summary>
    /// Let the bodies-
    /// </summary>
    class Cube : GameObject
    {
       Mesh cube;
       public Cube(Material material, PhysicsManager physics)
        {

	        PhysicsTick = true;
	        
	        
	        
	        cube = new Mesh(this, material);
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
            cube.GenerateMesh(data);
            material.AddReference(cube);
            
        }

        public override unsafe void _PhysicsProcess(double delta)
        {
	        
	        
        }
    }


}