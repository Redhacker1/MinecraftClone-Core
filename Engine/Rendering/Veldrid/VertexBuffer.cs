using System;
using Veldrid;

namespace Engine.Rendering.Veldrid
{

    public class VertexBuffer<TDataType> : BaseBufferTyped<TDataType>
        where TDataType : unmanaged
    {
        
        public unsafe VertexBuffer(GraphicsDevice gDevice, ReadOnlySpan<TDataType> data)
        {
            SafeCreateBuffer(gDevice, (uint)data.Length);
            ModifyBuffer(data, gDevice);
        }


        protected override void OnDispose()
        {
            BufferObject.Dispose();
        }

        internal override void Bind(CommandList list, uint slot = 0)
        {
            list.SetVertexBuffer(slot, BufferObject);
        }
    }
}