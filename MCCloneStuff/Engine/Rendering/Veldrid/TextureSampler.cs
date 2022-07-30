using Veldrid;

namespace Engine.Rendering.Veldrid
{
    public class TextureSampler : IGraphicsResource
    {

        Sampler _sampler;

        public TextureSampler(Sampler samplertype)
        {
            _sampler = samplertype;
        }

        internal override (ResourceKind, BindableResource) GetUnderlyingResource()
        {
            return (ResourceKind.Sampler, _sampler);
        }

        protected override void OnDispose()
        {
            
        }
    }
}