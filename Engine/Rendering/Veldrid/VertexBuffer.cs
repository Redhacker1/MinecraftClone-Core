using System;
using Veldrid;

namespace Engine.Rendering.Veldrid
{

    public class VertexBuffer<TDataType> : BaseBufferTyped<TDataType>
        where TDataType : unmanaged
    {
        
        public unsafe VertexBuffer(GraphicsDevice gDevice, ReadOnlySpan<TDataType> data) : base(gDevice)
        {
            BufferType = BufferUsage.VertexBuffer;
            SafeCreateBuffer(gDevice, (uint)data.Length);
            ModifyBuffer(data, gDevice);
        }


        protected override void OnDispose()
        {
            _device.DisposeWhenIdle(BufferObject);
        }

        internal override void Bind(CommandList list, uint slot = 0)
        {
            list.SetVertexBuffer(slot, BufferObject);
        }
    }
}