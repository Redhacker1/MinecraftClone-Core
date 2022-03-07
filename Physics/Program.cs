// See https://aka.ms/new-console-template for more information

using System.Collections.Generic;
using BulletSharp;
using BulletSharp.Math;
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
        Material _material;
        Camera cam = new Camera(new Vector3(10, 1, 1), Vector3.UnitZ, Vector3.UnitY,1024/768, true );
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
			
			
			var atlas = new Texture(WindowClass._renderer.Device, @"Assets\TextureAtlas.tga");
			var pointSampler = new TextureSampler(WindowClass._renderer.Device.PointSampler);
			
			_material.ResourceSet(0, WindowClass._renderer.ViewProjBuffer);
			_material.ResourceSet(1,WindowClass._renderer.WorldBuffer, pointSampler, atlas);
            
            _cube = new Cube(_material, _physicsManager);
        }
    }



    class PhysicsManager : GameObject
    {

	    AlignedCollisionObjectArray mCollisionShapes;
	    BroadphaseInterface mBroadphase = new DbvtBroadphase();
	    ConstraintSolver mSolver = new SequentialImpulseConstraintSolver();
	    DynamicsWorld mDynamicsWorld;
	    DefaultCollisionConfiguration CollisionConfiguration = new DefaultCollisionConfiguration();
	    CollisionDispatcher Dispatcher;
	    List<RigidBody> Bodies = new List<RigidBody>();
	    public PhysicsManager()
	    {
		    Dispatcher = new CollisionDispatcher(CollisionConfiguration);
	        mDynamicsWorld = new DiscreteDynamicsWorld(Dispatcher, mBroadphase, mSolver, CollisionConfiguration);
	        mDynamicsWorld.Gravity = new BulletSharp.Math.Vector3(0, -10, 0);
	        PhysicsTick = true;
        }
        public override void _PhysicsProcess(double delta)
        {
        }

        public void AddPhysicsBody(RigidBody body)
        {
	        
        }
    }

    class PhysicsBody
    {
        public PhysicsBody(PhysicsManager CollisionManager)
        {
            
        }
    }

    class Cube : GameObject
    {
        BulletMesh cube;
        RigidBody body;
        public Cube(Material material, PhysicsManager physics)
        {
	        var identity = Quaternion.Identity;
	        Matrix matrix = Matrix.Transformation(BulletSharp.Math.Vector3.Zero,identity, BulletSharp.Math.Vector3.One, BulletSharp.Math.Vector3.Zero, Quaternion.RotationYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z), new BulletSharp.Math.Vector3(Pos.X, Pos.Y, Pos.Z)  );
	        body = new RigidBody(new RigidBodyConstructionInfo(1, new DefaultMotionState(matrix), new BoxShape(.5f)));
	        
	        physics.AddPhysicsBody(body);
	        
	        cube = new BulletMesh(this, material);
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

        public override void _PhysicsProcess(double delta)
        {
	        
	        Matrix transform = body.MotionState.WorldTransform;
	        cube.BulletMatrix = transform;
        }
    }


}