using System;
using Veldrid;

namespace Engine.Rendering.Veldrid
{
    public abstract class GraphicsResource  : IDisposable
    {

        protected GraphicsDevice _device;
        public abstract (ResourceKind, BindableResource) GetUnderlyingResource();

        protected abstract void OnDispose();

        public virtual void Dispose()
        {
            OnDispose();
            GC.SuppressFinalize(this);
        }

        protected GraphicsResource(GraphicsDevice device)
        {
            _device = device;
        }
        

    }
}