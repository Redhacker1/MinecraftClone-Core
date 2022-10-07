using System;
using Veldrid;

namespace Engine.Rendering.Veldrid
{

    public abstract class BaseUnifomrBuffer
    {
        public abstract DeviceBuffer GetBuffer();
    }
    public unsafe class UniformBuffer<TDataType> : BaseUnifomrBuffer where TDataType : unmanaged
    {
        internal DeviceBuffer bufferObject;
        GraphicsDevice _device;
        public UniformBuffer(GraphicsDevice gDevice, Span<TDataType> data)
        {
            bufferObject =
                gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(TDataType) *  data.Length),
                    BufferUsage.UniformBuffer));
            ModifyBuffer(data, gDevice);
            _device = gDevice;
        }
        
        public UniformBuffer(GraphicsDevice gDevice, uint Length)
        {

            var size = sizeof(TDataType);
            size = size > 16 ? size : 16;
            
            bufferObject =
                gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( size * Length),
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _device = gDevice;
        }

        
        
        public uint Length => (uint) (bufferObject == null ? 0 : bufferObject.SizeInBytes / sizeof(TDataType));

        public void Dispose()
        {
            bufferObject.Dispose();
        }
        
        public void ModifyBuffer(ReadOnlySpan<TDataType> data, GraphicsDevice device)
        {
            if (data.Length > Length)
            {
                bufferObject.Dispose();
                
                BufferDescription bufferDescription = new BufferDescription((uint)(data.Length * sizeof(TDataType)), BufferUsage.UniformBuffer | BufferUsage.Dynamic);
                bufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);;
            }
            device.UpdateBuffer(bufferObject, 0, data);
        }
        
        public void ModifyBuffer(TDataType data, GraphicsDevice device)
        {
            if (1 > Length)
            {
                bufferObject.Dispose();
                
                BufferDescription bufferDescription = new BufferDescription((uint)(1 * sizeof(TDataType)), BufferUsage.UniformBuffer | BufferUsage.Dynamic);
                bufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);;
            }
            device.UpdateBuffer(bufferObject, 0, data);
        }
        
        public void ModifyBuffer(ReadOnlySpan<TDataType> data, CommandList list, GraphicsDevice device)
        {
            if (data.Length > Length)
            {
                bufferObject.Dispose();
                
                BufferDescription bufferDescription = new BufferDescription((uint) (data.Length * sizeof(TDataType)),
                    BufferUsage.UniformBuffer);
                bufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);
            }
            list.UpdateBuffer(bufferObject, 0, data);
        }
        
        public void ModifyBuffer(ReadOnlySpan<TDataType> data, CommandList list)
        {
            ModifyBuffer(data, list, _device);
        }
        
        public void ModifyBuffer(ReadOnlySpan<TDataType> updateMatrix)
        {
            ModifyBuffer(updateMatrix, _device);
        }

        public override DeviceBuffer GetBuffer()
        {
            return bufferObject;
        }
    }
}