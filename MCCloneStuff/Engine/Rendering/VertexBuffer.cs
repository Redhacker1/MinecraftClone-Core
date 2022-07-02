using System;
using Veldrid;

namespace Engine.Rendering
{
    public abstract class BaseVertexBuffer : IDisposable
    {
        internal abstract void Bind(CommandList list, uint slot = 0);

        public abstract void Dispose();

        public abstract void ModifyBuffer<T>(ReadOnlySpan<T> readOnlySpan, GraphicsDevice device) where T : unmanaged;
    }
    
    public class VertexBuffer<TDataType> : BaseVertexBuffer
        where TDataType : unmanaged
    {
        
        public unsafe VertexBuffer(GraphicsDevice gDevice, ReadOnlySpan<TDataType> data)
        {
            bufferObject = gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(TDataType) *  data.Length),
                BufferUsage.VertexBuffer));
            ModifyBuffer(data, gDevice);
            
        }

        DeviceBuffer bufferObject;
        public uint Length
        {
            get
            {
                unsafe
                {
                    return (uint) (bufferObject == null ? 0 : bufferObject.SizeInBytes / sizeof(TDataType)) ;
                }
            }
        }
        
        internal override void Bind(CommandList list, uint slot = 0)
        {
            list.SetVertexBuffer(0, bufferObject);
        }
        
        
        public override void Dispose()
        {
            bufferObject.Dispose();
        }
        
        public override void ModifyBuffer<T>(ReadOnlySpan<T> data, GraphicsDevice device)
        {
            if (data.Length > Length)
            {
                unsafe
                {
                    bufferObject.Dispose();
                
                    BufferDescription bufferDescription = new BufferDescription((uint)(data.Length * sizeof(TDataType)), BufferUsage.VertexBuffer);
                    bufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);;
                }
            }
            device.UpdateBuffer(bufferObject, 0, data);
        }
        
        
    }
}