using Veldrid;

namespace Engine.Rendering
{
    public interface IGraphicsResource
    {
        internal abstract (ResourceKind, BindableResource) GetUnderlyingResources();
    }
}