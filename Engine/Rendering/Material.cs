using System.Collections.Generic;
using Engine.Rendering;
using Engine.Windowing;
using Veldrid;
using Pipeline = Engine.Rendering.Pipeline;

namespace Engine.Renderable
{

    public abstract class MaterialBase
    {
        internal ResourceLayout[] layouts;
        internal ResourceSet[] Sets;

        internal abstract bool Bind(CommandList list);
        
        public void ResourceSet(uint slot, params IGraphicsResource[] resources)
        {
            var BindableResources = new BindableResource[resources.Length];

            for (int resource = 0; resource < resources.Length; resource++)
            {
                BindableResources[resource] = resources[resource].GetUnderlyingResources().Item2;
            }
            
            
            var Set = new ResourceSetDescription(layouts[slot], BindableResources);
            Sets[slot] = WindowClass._renderer.Device.ResourceFactory.CreateResourceSet(Set);
        }

    }
    public class Material : MaterialBase
    {
        internal static List<Material> _materials = new List<Material>();
        internal List<MaterialInstance> _instances = new List<MaterialInstance>();

        Pipeline pipeline_object;
        public Material(MaterialDescription description, VertexLayoutDescription vertexLayoutDescription, params ResourceLayout[] resourceLayouts)
        {
            layouts = resourceLayouts;
            pipeline_object = new Pipeline(description.DepthTest, description.WriteDepthBuffer,
                description.ComparisonKind,
                description.CullMode, description.FaceDir, description.Topology, description.FillMode,
                description.Shaders, WindowClass._renderer.Device, vertexLayoutDescription, resourceLayouts);
            
            Sets = new ResourceSet[layouts.Length];
        }


        internal override bool Bind(CommandList list)
        {
            if (pipeline_object._pipeline != null)
            {
         
                list.SetPipeline(pipeline_object._pipeline);

                for (int i = 0; i < Sets.Length; i++)
                {
                    if (Sets[i] != null)
                    {
                        list.SetGraphicsResourceSet((uint)i, Sets[i]);    
                    }
                }

                return true;
            }
            return false;
        }
    }

    class MaterialInstance : MaterialBase
    {
        Material baseMat;
        MaterialInstance(Material baseInstance)
        {
            baseMat = baseInstance;
        }
        

        internal override bool Bind(CommandList list)
        {
            return false;
        }
    }
}