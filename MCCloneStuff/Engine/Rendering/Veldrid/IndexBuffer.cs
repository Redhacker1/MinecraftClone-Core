using System;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Engine.Rendering.Veldrid
{

    public class IndexBuffer<T> : BaseBufferTyped<T>
        where T : unmanaged
    {
        
        public IndexBuffer(GraphicsDevice gDevice, Span<T> data)
        {
            BufferType = BufferUsage.IndexBuffer;
            unsafe
            {
                BufferObject = gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(T) *  data.Length), BufferUsage.IndexBuffer));
                ModifyBuffer(data, gDevice);
            }
        }
        public IndexBuffer(GraphicsDevice gDevice, int length)
        {
            BufferType = BufferUsage.IndexBuffer;
            unsafe
            {
                BufferObject = gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(T) *  length), BufferUsage.IndexBuffer));
            }
        }

        protected override void OnDispose()
        {
            BufferObject.Dispose();
        }

        internal override void Bind(CommandList list, uint slot = 0)
        {
            list.SetIndexBuffer(BufferObject, IndexFormat.UInt32);
        }

        internal override (ResourceKind, BindableResource) GetUnderlyingResource()
        {
            throw new NotSupportedException("Not usable on uniform buffers!");
        }
    }
}