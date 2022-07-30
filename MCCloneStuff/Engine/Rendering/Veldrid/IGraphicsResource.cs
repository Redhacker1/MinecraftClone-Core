using System;
using Veldrid;

namespace Engine.Rendering.Veldrid
{
    public abstract class IGraphicsResource : IDisposable
    {
        internal abstract (ResourceKind, BindableResource) GetUnderlyingResource();

        protected abstract void OnDispose();

        public void Dispose()
        {
            OnDispose();
            GC.SuppressFinalize(this);
        }
    }
}