#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using Engine;
using Engine.AssetLoading.AssimpIntegration;
using Engine.Debugging;
using Engine.Initialization;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using Engine.Rendering.VeldridBackend;
using Engine.Windowing;
using Silk.NET.Input;
using Veldrid;
using Texture = Engine.Rendering.VeldridBackend.Texture;

namespace ObjDemo
{
	internal class ObjDemo : GameEntry
	{
		ConsoleText debug_console = new ConsoleText();
		MeshSpawner thing;

		protected override void GameStart()
		{
			base.GameStart();

			DebugPanel panel = new DebugPanel();
			Camera fpCam = new Camera(new Transform(), -Vector3.UnitZ, Vector3.UnitX, 1.77777F, true);
			thing = new MeshSpawner();



			ConsoleLibrary.InitConsole(debug_console.SetConsoleScrollback);

			InputHandler.SetMouseMode(0, CursorMode.Normal);


			PinnedObject = thing;

		}
	}

	internal class Entrypoint
	{
		static void Main()
		{
			Init.InitEngine(0, 0, 1600, 900, "Model Viewer", new ObjDemo());
		}
	}

	internal class MeshSpawner : BaseLevel
	{
		Player player;
		Mesh[] _meshes;
		Material[] Materials;
		Material baseMaterial;
		Texture atlas;
		RotationPanel panel;

		AssimpNodeTree Node;


		unsafe void CreateBaseMaterial()
		{
			ShaderSet set = new ShaderSet(
				ShaderExtensions.CreateShaderFromFile(ShaderType.Vertex, "./Assets/shader.vert", "main", ShaderExtensions.ShadingLanguage.GLSL),
				ShaderExtensions.CreateShaderFromFile(ShaderType.Fragment, "./Assets/shader.frag", "main", ShaderExtensions.ShadingLanguage.GLSL));
			
			MaterialDescription materialDescription = new MaterialDescription
			{
				BlendState = BlendStateDescription.SingleDisabled,
				ComparisonKind = ComparisonKind.LessEqual,
				CullMode = FaceCullMode.Back,
				Topology = PrimitiveTopology.TriangleList,
				DepthTest = true,
				WriteDepthBuffer = true,
				FaceDir = FrontFace.CounterClockwise,
				FillMode = PolygonFillMode.Solid,
				Shaders = set
			};


			ResourceLayoutDescription ProjectionLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer,
					ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly,
					ShaderStages.Fragment));

			baseMaterial = new Material(materialDescription, 
				new[] {
					new VertexLayoutDescription((uint)sizeof(Matrix4x4), 1, 
						new VertexElementDescription("Matrix1xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
						new VertexElementDescription("Matrix2xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
						new VertexElementDescription("Matrix3xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
						new VertexElementDescription("Matrix4xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)),
					
					new VertexLayoutDescription(
						new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
							VertexElementFormat.Float3),
						new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate,
							VertexElementFormat.Float3)
						
					)
				}
				, Engine.Engine.Renderer,
				
				new []
				{
					ProjectionLayout,
					fragmentLayout	
				}

			);

			atlas = new Texture(Engine.Engine.Renderer.Device, "Assets/TextureAtlas.tga");
			baseMaterial.ResourceSet(1, new TextureSampler(Engine.Engine.Renderer.Device.Aniso4xSampler), atlas);

			Scale = new Vector3(.1f);

		}

		public override void _Ready()
		{
			base._Ready();
			Ticks = true;
			LocalTransform = new Transform();


			AssimpScene? Scene = AssimpLoader.AssimpImport("Assets/Bistro/BistroExterior.fbx");
			
			Console.WriteLine("Setting up level");

			panel = new RotationPanel(this);
			CreateBaseMaterial();

			if (atlas._Texture == null)
			{
				throw new Exception("Texture is null");
			}

			player = new Player();

			AddChild(player);
			player._Ready();
			
			Node = new AssimpNodeTree(Scene, Scene.RootNode, new Transform(), baseMaterial);
			AddChild(Node);
			Node._Ready();

		}
	}

	class AssimpNodeTree : EngineObject
	{
		string Name;
		Mesh[] Meshes;
		Instance3D[] _instance3Ds;

		public AssimpNodeTree(AssimpScene scene, AssimpNode nodeDescription, Transform nodeOffset, Material material)
		{
			
			Name = nodeDescription.Name;

			Meshes = new Mesh[nodeDescription.MeshIndices.Length];
			_instance3Ds = new Instance3D[nodeDescription.MeshIndices.Length];

			SetTransform(nodeOffset);


			if (nodeDescription.MeshIndices != Array.Empty<uint>())
			{
				for (int location = 0; location < nodeDescription.MeshIndices.Length; location++)
				{
					uint index = nodeDescription.MeshIndices[location];
					
					
					AssimpMesh mesh = scene.Meshes[index];
					
					Meshes[location] = new Mesh();
					List<uint> meshIndices = new List<uint>();

					foreach (uint[] t in mesh.Indices)
					{
						meshIndices.AddRange(t);
					}

					Meshes[location].GenerateMesh(mesh.Vertices, mesh.TextureCoords[0], meshIndices.ToArray());
					_instance3Ds[location] = new Instance3D(Meshes[location], material);


					AddChild(_instance3Ds[location]);
					WindowClass.Renderer.Stages[0].AddInstance(_instance3Ds[location]);
				}
			}
			foreach (AssimpNode child in nodeDescription.Children)
			{

				AddChild(new AssimpNodeTree(scene, child, child.Transform, material));
			}

		}
	}
}