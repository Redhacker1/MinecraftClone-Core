using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Renderable;
using Engine.Rendering.Culling;
using Engine.Windowing;
using Veldrid;

namespace Engine.Rendering
{

    public class Material
    {
        internal static ThreadSafeList<Material> _materials = new ThreadSafeList<Material>();
        internal List<Material> children = new List<Material>();
        Material parent;

        
        internal ResourceLayout[] layouts;
        internal ResourceSet[] Sets;
        internal ThreadSafeList<Renderable.Renderable> _references = new ThreadSafeList<Renderable.Renderable>();



        Pipeline pipeline_object;

        internal bool Bind(CommandList list)
        {
            if (pipeline_object != default)
            {
                list.SetPipeline(pipeline_object._pipeline);
            }

            for (int i = 0; i < Sets.Length; i++)
            {
                if (Sets[i] != null)
                {
                    list.SetGraphicsResourceSet((uint) i , Sets[i]);
                }
                else if(parent != null)
                {
                    list.SetGraphicsResourceSet((uint) i, parent.Sets[i]);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }


        internal void Render(CommandList list, Frustrum frustum, Renderer renderer)
        {
            Span<Matrix4x4> worldmatrix = stackalloc Matrix4x4[1];
            Bind(list);
            _references.EnterReadLock();
            foreach (var renderable in _references)
            {
                if (renderable.ShouldRender(frustum))
                {
                    worldmatrix[0] = renderable.ViewMatrix;
                    renderer.WorldBuffer.ModifyBuffer(worldmatrix, list);

                    renderable.BindResources(list);
                    if (renderable.UseIndexedDrawing)
                    {
                        list.DrawIndexed(renderable.VertexElements);
                    }
                    else
                    {
                        list.Draw(renderable.VertexElements);
                    }
                }
            }
            _references.ExitReadLock();

            foreach (Material child in children)
            {
                child.Render(list, frustum, renderer);
            }
        }



        public Material(MaterialDescription description, VertexLayoutDescription vertexLayoutDescription, Renderer renderer, params ResourceLayoutDescription[] resourceLayouts)
        {
            _materials.Add(this);
            ResourceLayout[] materialLayouts = new ResourceLayout[resourceLayouts.Length];

            for (int layoutIndex = 0; layoutIndex < resourceLayouts.Length; layoutIndex++)
            {
                ResourceLayoutDescription layout = resourceLayouts[layoutIndex];
                materialLayouts[layoutIndex] = renderer.Device.ResourceFactory.CreateResourceLayout(layout);
            }
            layouts = materialLayouts;
            
            
            pipeline_object = new Pipeline(description.DepthTest, description.WriteDepthBuffer,
                description.ComparisonKind,
                description.CullMode, description.FaceDir, description.Topology, description.FillMode,
                description.Shaders, renderer.Device, vertexLayoutDescription, layouts);

            Sets = new ResourceSet[materialLayouts.Length];
        }


        public Material(Material parent)
        {
            parent.children.Add(this);
            this.parent = parent;
            Sets = new ResourceSet[parent.Sets.Length];
            layouts = parent.layouts;
        }



        public void ResourceSet(uint slot, params IGraphicsResource[] resources)
        {
            BindableResource[] BindableResources = new BindableResource[resources.Length];

            for (int resource = 0; resource < resources.Length; resource++)
            {
                BindableResources[resource] = resources[resource].GetUnderlyingResources().Item2;
            }

            ResourceSetDescription Set = new ResourceSetDescription(layouts[slot], BindableResources);
            Sets[slot] = WindowClass._renderer.Device.ResourceFactory.CreateResourceSet(Set);
        }
        
        public void AddReference(Renderable.Renderable renderable)
        {
            _references.Add(renderable);
        }

        public void RemoveReference(Renderable.Renderable mesh)
        {
            _references.Remove(mesh);
        }

        //public void


    }
}