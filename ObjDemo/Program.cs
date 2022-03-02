#nullable enable
using System.Collections.Generic;
using System.Numerics;
using Engine;
using Engine.AssetLoading.Assimp;
using Engine.Debug;
using Engine.Initialization;
using Engine.Input;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Windowing;
using Silk.NET.Input;
using Veldrid;
using Shader = Engine.Rendering.Shader;
using Texture = Engine.Rendering.Texture;
using TextureType = Silk.NET.Assimp.TextureType;

namespace ObjDemo
{
    internal class ObjDemo : Game
    {
	    ConsoleText debug_console = new ConsoleText();
        public override void Gamestart()
        {
            base.Gamestart();

            DebugPanel panel = new DebugPanel();
            Camera fpCam = new Camera(new Vector3(0, 0, 0), -Vector3.UnitZ, Vector3.UnitX , 1.77777F, true );
            MeshSpawner thing = new MeshSpawner();
            
            
            
            ConsoleLibrary.InitConsole(debug_console.SetConsoleScrollback);

            InputHandler.SetMouseMode(0, CursorMode.Normal);
            
        }
    }

    internal class Entrypoint
    {
        static void Main()
        {
            Init.InitEngine( 0,0, 1600, 900, "Hello World", new ObjDemo());
        }
    }

    internal class MeshSpawner : GameObject
    {
	    Player player;
        Mesh[] _meshes;
        Material[] Materials;
        Material baseMaterial;
        Texture atlas;



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
						new Shader("./Assets/frag.spv", WindowClass._renderer.Device, ShaderStages.Fragment)
					},
					{
						ShaderStages.Vertex,
						new Shader("./Assets/vert.spv", WindowClass._renderer.Device, ShaderStages.Vertex)
					}


				}
			};

			ResourceLayoutDescription vertexLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));
			
			baseMaterial = new Material(materialDescription, 
				new VertexLayoutDescription(
					new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
					new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
				,WindowClass._renderer,
				
				vertexLayout,
				fragmentLayout

				);
			
			atlas = new Texture(WindowClass._renderer.Device, @"Assets\TextureAtlas.tga");
			baseMaterial.ResourceSet(0, WindowClass._renderer.ViewProjBuffer);
			baseMaterial.ResourceSet(1, WindowClass._renderer.WorldBuffer, new TextureSampler(WindowClass._renderer.Device.Aniso4xSampler), atlas);
        }

        public override void _Ready()
        {
	        Rotation = new Vector3(0, -90, 0);

	        RotationPanel panel = new RotationPanel(this);
            base._Ready();
            CreateBaseMaterial();
            
            
            
            
            
            
            //ImGUI_ModelViewer viewer = new ImGUI_ModelViewer();
            player = new Player();

            (AssimpMeshBuilder[]? assimpMeshBuilders, AssimpMaterialStruct[]? assimpMaterialStructs) = AssimpLoader.LoadMesh(@"Assets\Bistro\BistroExterior.fbx");
	        _meshes = new Mesh[assimpMeshBuilders.Length];
	        Materials = new Material[assimpMaterialStructs.Length];
	        uint[] meshMaterialMap = new uint[assimpMeshBuilders.Length];

	        for (int materialIndex = 0; materialIndex < Materials.Length; materialIndex++)
	        {
		        Materials[materialIndex] = new Material(baseMaterial);
		        
		        AssimpMaterialStruct ASSIMPmaterial = assimpMaterialStructs[materialIndex];
		        var Diffuse = ASSIMPmaterial._textures[TextureType.TextureTypeDiffuse];

		        //Console.WriteLine(Diffuse[0].path);

		        Materials[materialIndex].ResourceSet(1, WindowClass._renderer.WorldBuffer, new TextureSampler(WindowClass._renderer.Device.Aniso4xSampler), Diffuse[0]._texture); 
	        }
	        

	        for (int meshIndex = 0; meshIndex < _meshes.Length; meshIndex++)
	        {
		        AssimpMeshBuilder currentassimpMesh = assimpMeshBuilders[meshIndex];
		        _meshes[meshIndex] = new Mesh(this, Materials[currentassimpMesh.MaterialIndex]);
		        _meshes[meshIndex].GenerateMesh(assimpMeshBuilders[meshIndex].Data);
		        meshMaterialMap[meshIndex] = currentassimpMesh.MaterialIndex;
		        _meshes[meshIndex].Scale = .1f;
		        Materials[meshMaterialMap[meshIndex]].AddReference(_meshes[meshIndex]);
	        }
        }
    }


}