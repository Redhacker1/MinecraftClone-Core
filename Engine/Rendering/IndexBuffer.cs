using System;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Engine.Rendering
{
    public abstract class BaseIndexBuffer
    {
        internal abstract void Bind(CommandList list);
    }
    
    public class IndexBuffer<TDataType> : BaseIndexBuffer,  IDisposable
        where TDataType : unmanaged
    {
        
        public IndexBuffer(GraphicsDevice gDevice, Span<TDataType> data)
        {
            unsafe
            {
                bufferObject = gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(TDataType) *  data.Length), BufferUsage.IndexBuffer));
                ModifyBuffer(data, gDevice);
            }
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
        
        internal override void Bind(CommandList list)
        {
            list.SetIndexBuffer(bufferObject, IndexFormat.UInt32);
        }
        
        
        public void Dispose()
        {
            bufferObject.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ModifyBuffer(Span<TDataType> data, GraphicsDevice device)
        {
            if (data.Length > Length)
            {
                unsafe
                {
                    bufferObject.Dispose();
                
                    BufferDescription bufferDescription = new BufferDescription((uint)(data.Length * sizeof(TDataType)), BufferUsage.IndexBuffer);
                    bufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);;
                }
            }
            device.UpdateBuffer(bufferObject, 0, data.ToArray());
        }
    }
}