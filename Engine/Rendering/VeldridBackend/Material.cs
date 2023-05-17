using Veldrid;

namespace Engine.Rendering.VeldridBackend
{

    public class Material
    {
        Material _parent;
        internal ResourceLayout[] Layouts;
        public ResourceSet[] Sets;

        Pipeline _pipeline;

        internal bool Bind(CommandList list, ResourceSet setZero, bool depthPrepass = false)
        {
            if (_pipeline != new Pipeline() && depthPrepass == false)
            {
                list.SetPipeline(_pipeline._pipeline);
            }

            list.SetGraphicsResourceSet(0 , setZero);

            if (!depthPrepass)
            {
                for (int i = 1; i < Sets.Length; i++)
                {
                    if (Sets[i] != null)
                    {
                        list.SetGraphicsResourceSet((uint) i , Sets[i]);
                    }
                    else if(_parent != null)
                    {
                        list.SetGraphicsResourceSet((uint) i, _parent.Sets[i]);
                    }
                    else
                    {
                        return false;
                    }
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
            Layouts = materialLayouts;

            _pipeline = new Pipeline(description.DepthTest, true,
                ComparisonKind.Less,
                description.CullMode, description.FaceDir, description.Topology, description.FillMode,
                description.Shaders, renderer.Device, vertexLayoutDescription, Layouts);


            Sets = new ResourceSet[materialLayouts.Length];
        }
        
        public Material(Material parent)
        {
            this._parent = parent;
            Sets = new ResourceSet[parent.Sets.Length];
            Layouts = parent.Layouts;
        }
        
        public void ResourceSet(uint slot, params GraphicsResource[] resources)
        {
            BindableResource[] bindableResources = new BindableResource[resources.Length];
            for (int resource = 0; resource < resources.Length; resource++)
            {
                bindableResources[resource] = resources[resource].GetUnderlyingResource().Item2;
            }

            if (slot < Layouts.Length && Layouts[slot] != null)
            {
                ResourceSetDescription set = new ResourceSetDescription(Layouts[slot], bindableResources);   
                Sets[slot] = Engine.Renderer.Device.ResourceFactory.CreateResourceSet(set);
            }
        }


    }
}