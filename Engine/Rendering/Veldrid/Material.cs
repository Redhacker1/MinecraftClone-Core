using Engine.Windowing;
using Veldrid;

namespace Engine.Rendering.Veldrid
{

    public class Material
    {
        Material parent;
        internal ResourceLayout[] layouts;
        internal ResourceSet[] Sets;

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

        public Material(MaterialDescription description, VertexLayoutDescription[] vertexLayoutDescription, Renderer renderer, ResourceLayoutDescription[] resourceLayouts)
        {
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
            this.parent = parent;
            Sets = new ResourceSet[parent.Sets.Length];
            layouts = parent.layouts;
        }
        
        public void ResourceSet(uint slot, params IGraphicsResource[] resources)
        {
            BindableResource[] bindableResources = new BindableResource[resources.Length];
            for (int resource = 0; resource < resources.Length; resource++)
            {
                bindableResources[resource] = resources[resource].GetUnderlyingResource().Item2;
            }

            if (slot < layouts.Length && layouts[slot] != null)
            {
                ResourceSetDescription set = new ResourceSetDescription(layouts[slot], bindableResources);   
                Sets[slot] = WindowClass.Renderer.Device.ResourceFactory.CreateResourceSet(set);
            }
        }


    }
}