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
        GraphicsDevice _device;
        public UniformBuffer(GraphicsDevice gDevice, Span<TDataType> data, string Name = "")
        {
            bufferObject =
                gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(TDataType) *  data.Length),
                    BufferUsage.UniformBuffer));
            ModifyBuffer(data, gDevice);
            _device = gDevice;
            bufferObject.Name = Name;
        }
        
        public UniformBuffer(GraphicsDevice gDevice, uint Length, string Name = "")
        {
            bufferObject =
                gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(TDataType) *  Length),
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _device = gDevice;
            bufferObject.Name = Name;
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
                bufferObject.Dispose();
                
                BufferDescription bufferDescription = new BufferDescription((uint)(data.Length * sizeof(TDataType)), BufferUsage.UniformBuffer | BufferUsage.Dynamic);
                bufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);;
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
        
        public void ModifyBuffer(Span<TDataType> data, CommandList list)
        {
            ModifyBuffer(data, list, _device);
        }
        
        public void ModifyBuffer(Span<TDataType> updateMatrix)
        {
            ModifyBuffer(updateMatrix, _device);
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