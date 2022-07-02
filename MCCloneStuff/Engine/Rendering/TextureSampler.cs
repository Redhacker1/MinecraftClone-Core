using Veldrid;

namespace Engine.Rendering
{
    public class TextureSampler : IGraphicsResource
    {

        Sampler _sampler;

        public TextureSampler(Sampler samplertype)
        {
            _sampler = samplertype;
        }
        
        (ResourceKind, BindableResource) IGraphicsResource.GetUnderlyingResources()
        {
            return (ResourceKind.Sampler, _sampler);
        }
    }
}