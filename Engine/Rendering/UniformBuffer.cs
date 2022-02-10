using System;
using Veldrid;

namespace Engine.Rendering
{

    public abstract class BaseUnifomrBuffer
    {
        internal abstract DeviceBuffer GetBuffer();
    }
    public unsafe class UniformBuffer<TDataType> : BaseUnifomrBuffer, IGraphicsResource, IDisposable where TDataType : unmanaged
    {
        internal DeviceBuffer bufferObject;
        public UniformBuffer(GraphicsDevice gDevice, Span<TDataType> data)
        {
            bufferObject =
                gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(TDataType) *  data.Length),
                    BufferUsage.UniformBuffer));
            ModifyBuffer(data, gDevice);
            
        }
        
        public UniformBuffer(GraphicsDevice gDevice, uint Length)
        {
            bufferObject =
                gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(TDataType) *  Length),
                    BufferUsage.UniformBuffer));
        }

        
        
        public uint Length => (uint) (bufferObject == null ? 0 : bufferObject.SizeInBytes / sizeof(TDataType));

        public void Dispose()
        {
            bufferObject.Dispose();
        }
        
        public void ModifyBuffer(Span<TDataType> data, GraphicsDevice device)
        {
            if (data.Length > Length)
            {
                unsafe
                {
                    bufferObject.Dispose();
                
                    BufferDescription bufferDescription = new BufferDescription((uint)(data.Length * sizeof(TDataType)), BufferUsage.UniformBuffer);
                    bufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);;
                }
            }
            device.UpdateBuffer(bufferObject, 0, data);
        }
        
        public void ModifyBuffer(Span<TDataType> data, CommandList list, GraphicsDevice device)
        {
            if (data.Length > Length)
            {
                bufferObject.Dispose();
                
                BufferDescription bufferDescription = new((uint)(data.Length * sizeof(TDataType)), BufferUsage.UniformBuffer);
                bufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);
            }
            list.UpdateBuffer(bufferObject, 0, data);
        }

        internal override DeviceBuffer GetBuffer()
        {
            return bufferObject;
        }

        (ResourceKind, BindableResource) IGraphicsResource.GetUnderlyingResources()
        {
            return (ResourceKind.UniformBuffer, bufferObject);
        }
    }
}