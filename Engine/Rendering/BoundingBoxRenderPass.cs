using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Renderable;
using Engine.Windowing;
using Veldrid;

namespace Engine.Rendering
{
    public class BoundingBoxRenderPass : Renderpass
    {
        Material AABBMaterial;

        public BoundingBoxRenderPass(CommandList _list, Renderer renderer) : base(_list, renderer)
        {
            CreateResources();
        }

        public BoundingBoxRenderPass(Renderer renderer) : base(renderer)
        {
            CreateResources();
        }



        void CreateResources()
        {
            
        }

        void CreateMaterial()
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
			
            AABBMaterial = new Material(materialDescription, 
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
                ,WindowClass._renderer,
				
                vertexLayout,
                fragmentLayout

            );
        }
        
        
        

        // TODO: use instancing to render all of these AABB's in one draw-call and automatically cull out AABBs not in frustum!
        // TODO: (We have the camera, AND the AABB (duh ;p), might as well!)
        protected override void Pass(CommandList list)
        {
            Span<Vector3> minmax = stackalloc Vector3[2];

            Span<Vector3> boxverts = stackalloc Vector3[7];
            
            // The indices never change, there is no reason to update them, in fact we could make this a
            // static readonly array once I can verify it does in fact work, that way it do not take up stack memory space
            Span<uint> indices = stackalloc uint[24]
            {
                1,2,3,4,
                2,5,6,3,
                4,3,6,7,
                8,7,6,5,
                8,5,2,1,
                8,1,4,7,
            };

            IndexBuffer<uint> indexBuffer = new IndexBuffer<uint>(backingRenderer.Device, indices);
            VertexBuffer<Vector3> boxBuffer = new VertexBuffer<Vector3>(backingRenderer.Device,boxverts);
            
            

            for (int meshIndex = 0; meshIndex < Mesh.Meshes.Count; meshIndex++)
            {
                Mesh currentmesh = Mesh.Meshes[meshIndex];
                currentmesh.GetMinMaxScaled(minmax, Camera.MainCamera.Pos);
                
                
                
                
            }
        }
    }
}




// Vertices and corresponding indices from MinMax representation to 24-point for rendering
/*
========================
Unique Vertices and corresponding indices (0-indexed) 
========================
Min.X, Min.Y, Max.Z - 0
Max.X, Min.Y, Max.Z - 1
Max.X, Max.Y, Max.Z - 2
Min.X, Max.Y, Max.Z - 3
Max.X, Min.Y, Min.Z - 4
Max.X, Max.Y, Min.Z - 5
Min.X, Max.Y, Min.Z - 6
Min.X, Min.Y, Min.Z - 7
 
========================
Face vertices and corresponding indices (1-indexed NOT 0 indexed) 
========================
Min.X, Min.Y, Max.Z - 1
Max.X, Min.Y, Max.Z - 2
Max.X, Max.Y, Max.Z - 3
Min.X, Max.Y, Max.Z - 4

Max.X, Min.Y, Max.Z - 2
Max.X, Min.Y, Min.Z - 5
Max.X, Max.Y, Min.Z - 6
Max.X, Max.Y, Max.Z - 3

Min.X, Max.Y, Max.Z - 4
Max.X, Max.Y, Max.Z - 3
Max.X, Max.Y, Min.Z - 6
Min.X, Max.Y, Min.Z - 7

Min.X, Min.Y, Min.Z - 8
Min.X, Max.Y, Min.Z - 7
Max.X, Max.Y, Min.Z - 6
Max.X, Min.Y, Min.Z - 5

Min.X, Min.Y, Min.Z - 8
Max.X, Min.Y, Min.Z - 5
Max.X, Min.Y, Max.Z - 2
Min.X, Min.Y, Max.Z - 1

Min.X, Min.Y, Min.Z - 8
Min.X, Min.Y, Max.Z - 1
Min.X, Max.Y, Max.Z - 4
Min.X, Max.Y, Min.Z - 7
*/