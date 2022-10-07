using System;
using System.Collections.Generic;
using Veldrid;

namespace Engine.Rendering.Veldrid
{
    public abstract class GraphicsResource  : IDisposable
    {
        public abstract (ResourceKind, BindableResource) GetUnderlyingResource();

        protected abstract void OnDispose();

        public virtual void Dispose()
        {
            OnDispose();
            GC.SuppressFinalize(this);
        }

        protected GraphicsResource()
        {
            
        }
        

    }
}