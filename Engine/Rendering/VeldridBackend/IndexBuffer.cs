using System;
using Veldrid;

namespace Engine.Rendering.VeldridBackend
{

    public class IndexBuffer<T> : BaseBufferTyped<T>
        where T : unmanaged
    {
        
        public IndexBuffer(GraphicsDevice gDevice, ReadOnlySpan<T> data) : base(gDevice)
        {
            BufferType = BufferUsage.IndexBuffer;
            unsafe
            {
                BufferObject = gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(T) *  data.Length), BufferUsage.IndexBuffer));
                ModifyBuffer(data, gDevice);
            }
        }
        public IndexBuffer(GraphicsDevice gDevice, int length) : base(gDevice)
        {
            BufferType = BufferUsage.IndexBuffer;
            unsafe
            {
                BufferObject = gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(T) *  length), BufferUsage.IndexBuffer));
            }
        }

        protected override void OnDispose()
        {
            _device.DisposeWhenIdle(BufferObject);
        }

        internal override void Bind(CommandList list, uint slot = 0)
        {
            list.SetIndexBuffer(BufferObject, IndexFormat.UInt32);
        }

        public override (ResourceKind, BindableResource) GetUnderlyingResource()
        {
            throw new NotSupportedException("Not usable on uniform buffers!");
        }
    }
}