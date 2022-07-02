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
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using Silk.NET.Input;
using Veldrid;
using Shader = Engine.Rendering.Veldrid.Shader;
using Texture = Engine.Rendering.Veldrid.Texture;

namespace ObjDemo
{
	internal class ObjDemo : Game
	{
		ConsoleText debug_console = new ConsoleText();

		public override void Gamestart()
		{
			base.Gamestart();

			DebugPanel panel = new DebugPanel();
			Camera fpCam = new Camera(new Vector3(0, 0, 0), -Vector3.UnitZ, Vector3.UnitX, 1.77777F, true);
			MeshSpawner thing = new MeshSpawner();



			ConsoleLibrary.InitConsole(debug_console.SetConsoleScrollback);

			InputHandler.SetMouseMode(0, CursorMode.Normal);

		}
	}

	internal class Entrypoint
	{
		static void Main()
		{
			Init.InitEngine(0, 0, 1600, 900, "Hello World", new ObjDemo());
		}
	}

	internal class MeshSpawner : GameObject
	{
		Player player;
		Mesh[] _meshes;
		Material[] Materials;
		Material baseMaterial;
		Texture atlas;

		AssimpNodeTree Node;

		List<GameObject> Nodes = new List<GameObject>();



		void CreateBaseMaterial()
		{
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


			ResourceLayoutDescription ProjectionLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer,
					ShaderStages.Vertex));
			ResourceLayoutDescription ModelLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ModelProjBuffer", ResourceKind.UniformBuffer,
					ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly,
					ShaderStages.Fragment));

			baseMaterial = new Material(materialDescription,
				new VertexLayoutDescription(
					new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
						VertexElementFormat.Float3),
					new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate,
						VertexElementFormat.Float3))
				, WindowClass.Renderer,

				ProjectionLayout,
				ModelLayout,
				fragmentLayout

			);

			atlas = new Texture(WindowClass.Renderer.Device, @"Assets\TextureAtlas.tga");
			baseMaterial.ResourceSet(2, new TextureSampler(WindowClass.Renderer.Device.Aniso4xSampler), atlas);
		}

		public override void _Ready()
		{
			Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.DegreesToRadians(0),
				MathHelper.DegreesToRadians(0), MathHelper.DegreesToRadians(0));

			RotationPanel panel = new RotationPanel(this);
			base._Ready();
			CreateBaseMaterial();

			if (atlas._Texture == null)
			{
				throw new Exception("Texture is null");
			}

			player = new Player();

			var Scene = AssimpLoader.AssimpImport(@"Assets/Bistro/BistroExterior.fbx");

			Node = new AssimpNodeTree(Scene, Scene.RootNode, Matrix4x4.Identity, baseMaterial);
		}
	}

	class AssimpNodeTree : GameObject
	{
		string Name;
		AssimpNodeTree[] Children;
		Mesh[] Meshes;

		public AssimpNodeTree(AssimpScene scene, AssimpNode nodeDescription, Matrix4x4 nodeOffset, Material material)
		{

			Name = nodeDescription.Name;

			Meshes = new Mesh[nodeDescription.MeshIndices.Length];

			Matrix4x4 nodeTransform = nodeOffset * nodeDescription.Transform;

			Matrix4x4.Decompose(nodeTransform, out Vector3 nodeScale, out Quaternion nodeRotation, out Vector3 nodePos);
			SetTransform(nodePos, nodeRotation, nodeScale);


			if (nodeDescription.MeshIndices != Array.Empty<uint>())
			{
				int location = 0;
				foreach (uint indices in nodeDescription.MeshIndices)
				{
					AssimpMesh Mesh = scene.Meshes[indices];
					Meshes[location] = new Mesh();
					List<uint> MeshIndices = new List<uint>();

					for (int FaceIndex = 0; FaceIndex < Mesh.Indices.Length; FaceIndex++)
					{
						MeshIndices.AddRange(Mesh.Indices[FaceIndex]);
					}

					Meshes[location].GenerateMesh(Mesh.Vertices, Mesh.TextureCoords[0], MeshIndices.ToArray());
					location++;
				}
			}
			if (nodeDescription.Children.Length > 0)
			{
				Children = new AssimpNodeTree[nodeDescription.Children.Length];
				for (int childIndex = 0; childIndex < nodeDescription.Children.Length; childIndex++)
				{
					Children[childIndex] = new AssimpNodeTree(scene, nodeDescription.Children[childIndex], GetTransform(),
						material);
				}
			}

		}
	}
}