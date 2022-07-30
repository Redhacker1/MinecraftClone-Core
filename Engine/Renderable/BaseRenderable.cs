using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Collision;
using Engine.Rendering.Veldrid;
using Veldrid;
using Vulkan;
using static System.GC;

namespace Engine.Renderable
{
    /// NOTE: Looks pretty much agnostic, how much can we move out into a 3D version and move 3D functionality in there?
    /// <summary>
    /// Engine type for referring to a generic Model. 
    /// </summary>
    public abstract class BaseRenderableUntyped : IDisposable
    {

        public bool UseIndexedDrawing {get; protected set; }
        public bool Disposed {get; internal set; }
        
        public uint VertexElements;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void GetMinMax(out Vector3 minPoint, out Vector3 maxPoint);
        
        protected BaseRenderableUntyped()
        {
            
        }

        internal abstract void BindResources(CommandList list);
        
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
    }


    public abstract class BaseRenderable<T> :BaseRenderableUntyped where T : unmanaged
    {

        protected IndexBuffer<uint> ebo;
        protected BaseBufferTyped<T> vbo;



        internal override void BindResources(CommandList list)
        {

            if (vbo.BufferType != BufferUsage.VertexBuffer || ebo?.BufferType == 0 && Disposed == false)
            {
                //Buffer has not been initialized, not an error, just has not been initialized,
                //should not trip, that being said, if run on another thread it might, and just covering bases, if it does we just need to skip ahead and move along
                if (vbo.BufferType == 0 || ebo.BufferType == 0)
                {
                    return;
                }
                
                throw new InvalidOperationException("Cannot bind non vertex or non index buffer!");
            }

            if (UseIndexedDrawing)
            {
                ebo?.Bind(list);   
            }
            vbo.Bind(list, 1);
            
        }

        protected override void ReleaseEngineResources()
        {
            ebo?.Dispose();
            vbo?.Dispose();   
        }

        ~BaseRenderable()
        {
            Console.WriteLine("Mesh object leak!");
        }
    }
}