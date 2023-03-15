using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Collision.Simple;
using Engine.Rendering.VeldridBackend;
using Veldrid;
using static System.GC;

namespace Engine.Renderable
{
    /// NOTE: Looks pretty much agnostic, how much can we move out into a 3D version and move 3D functionality in there?
    /// <summary>
    /// Engine type for referring to a generic Model. 
    /// </summary>
    public abstract class BaseRenderable : IDisposable
    {
        public bool Disposed {get; internal set; }
        
        public uint VertexElements;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void GetMinMax(out Vector3 minPoint, out Vector3 maxPoint);
        
        protected BaseRenderable()
        {
            
        }

        protected internal abstract void BindResources(CommandList list);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB GetBoundingBox()
        {
            AABB bbox =new AABB();
            GetMinMax(out Vector3 minPoint, out Vector3 maxPoint);
            bbox.SetAABB(minPoint, maxPoint);
            return bbox;
        }

        protected virtual void ReleaseEngineResources()
        {
            
        }

        protected virtual void OnDisposed()
        {
            return;
        }

        protected internal abstract void Draw(CommandList list, uint count, uint start);

        public void Dispose()
        {
            SuppressFinalize(this);
            if (Disposed == false)
            {
                Disposed = true;
                ReleaseEngineResources();
                OnDisposed();
            }
        }

        ~BaseRenderable()
        {
            Dispose();
        }
    }
    
}