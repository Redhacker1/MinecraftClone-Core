using Veldrid;

namespace Engine.Rendering.Veldrid
{
    public class TextureSampler : GraphicsResource
    {

        Sampler _sampler;

        public TextureSampler(Sampler samplertype) : base(null)
        {
            _sampler = samplertype;
        }

        public override (ResourceKind, BindableResource) GetUnderlyingResource()
        {
            return (ResourceKind.Sampler, _sampler);
        }

        protected override void OnDispose()
        {
            
        }
    }
}