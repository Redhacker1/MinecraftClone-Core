using Engine.Windowing;
using Veldrid;
using Pipeline = Engine.Rendering.Pipeline;

namespace Engine.Renderable
{
    public class Material
    {
        Pipeline pipeline_object;
        public Material(MaterialDescription description, VertexLayoutDescription vertexLayoutDescription, params ResourceLayout[] resourceLayouts)
        {
            layouts = resourceLayouts;
            pipeline_object = new Pipeline(description.DepthTest, description.WriteDepthBuffer,
                description.ComparisonKind,
                description.CullMode, description.FaceDir, description.Topology,
                description.Shaders, WindowClass._renderer.Device, vertexLayoutDescription, resourceLayouts);
            
            Sets = new ResourceSet[layouts.Length];
        }

        public ResourceSet[] Sets;
        public ResourceLayout[] layouts;

        
        

        public void ChangeMaterialProperties(ResourceSet set, int index = 0)
        {
            Sets[index] = set;
        }


        internal bool BindMaterial(CommandList list)
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
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }
        

    }
    
}