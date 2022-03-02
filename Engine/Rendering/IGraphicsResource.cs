using Veldrid;

namespace Engine.Rendering
{
    public interface IGraphicsResource
    {
        internal (ResourceKind, BindableResource) GetUnderlyingResources();
    }
}