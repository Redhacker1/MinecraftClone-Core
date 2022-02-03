#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Engine;
using Engine.AssetLoading;
using Engine.Initialization;
using Engine.Input;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Engine.Windowing;
using Silk.NET.Input;
using Veldrid;
using Shader = Engine.Rendering.Shader;
using Texture = Engine.Rendering.Texture;

namespace ObjDemo
{
    internal class ObjDemo : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();

            DebugPanel panel = new DebugPanel();
            Camera fpCam = new Camera(new Vector3(0, 0, 0), -Vector3.UnitZ, Vector3.UnitX , 1.77777F, true );
            MeshSpawner thing = new MeshSpawner();
            
            
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
        Mesh[] _meshes;

        public override void _Ready()
        {

	        RotationPanel panel = new RotationPanel(this);
            base._Ready();
            
            
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
				new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));

				var atlas = new Texture(WindowClass._renderer.Device, @"Assets\TextureAtlas.tga");
			var _material = new Material(materialDescription, 
				new VertexLayoutDescription(
					new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
					new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
				, 
				WindowClass._renderer.Device.ResourceFactory.CreateResourceLayout(vertexLayout),
				WindowClass._renderer.Device.ResourceFactory.CreateResourceLayout(fragmentLayout)

				);
			_material.Sets[0] = WindowClass._renderer.Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_material.layouts[0], WindowClass._renderer.ProjectionBuffer, WindowClass._renderer.ViewBuffer));	
			_material.Sets[1] = WindowClass._renderer.Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_material.layouts[1],WindowClass._renderer.WorldBuffer, WindowClass._renderer.Device.Aniso4xSampler, atlas._texture));

			//_material.ResourceSet(0, );
            
            
            
            //ImGUI_ModelViewer viewer = new ImGUI_ModelViewer();
            new Player();

            (Mesh[], IReadOnlyDictionary<string, Texture>) outAssimpValues = AssimpLoader.LoadMesh(@"Assets\Bistro\BistroExterior.fbx", this, _material);
            _meshes = outAssimpValues.Item1;

            foreach (var mesh in _meshes)
            {
	            Rotation = new Engine.MathLib.DoublePrecision_Numerics.Vector3(0, 200, 0);
                mesh.Scale = .1f;
                //mesh.GenerateMesh();  
            }
        }
    }


}