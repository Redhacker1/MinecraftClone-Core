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
        internal DeviceBuffer BufferObject;
        readonly GraphicsDevice _device;

        string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                BufferObject.Name = value;
            }
        }
        
        public UniformBuffer(GraphicsDevice gDevice, Span<TDataType> data)
        {
            BufferObject =
                gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( sizeof(TDataType) *  data.Length),
                    BufferUsage.UniformBuffer));
            ModifyBuffer(data, gDevice);
            _device = gDevice;
        }
        
        public UniformBuffer(GraphicsDevice gDevice, uint Length)
        {

            var size = sizeof(TDataType);
            size = size > 16 ? size : 16;
            
            BufferObject =
                gDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( size * Length),
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _device = gDevice;
        }

        
        
        public uint Length => (uint) (BufferObject == null ? 0 : BufferObject.SizeInBytes / sizeof(TDataType));

        public void Dispose()
        {
            BufferObject.Dispose();
        }
        
        public void ModifyBuffer(ReadOnlySpan<TDataType> data, GraphicsDevice device)
        {
            if (data.Length > Length)
            {
                BufferObject.Dispose();
                
                BufferDescription bufferDescription = new BufferDescription((uint)(data.Length * sizeof(TDataType)), BufferUsage.UniformBuffer | BufferUsage.Dynamic);
                BufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);
                BufferObject.Name = _name;
            }
            device.UpdateBuffer(BufferObject, 0, data);
        }
        
        public void ModifyBuffer(TDataType data, GraphicsDevice device)
        {
            if (1 > Length)
            {
                BufferObject.Dispose();
                
                BufferDescription bufferDescription = new BufferDescription((uint)(1 * sizeof(TDataType)), BufferUsage.UniformBuffer | BufferUsage.Dynamic);
                BufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);
                BufferObject.Name = _name;
            }
            device.UpdateBuffer(BufferObject, 0, data);
        }
        
        public void ModifyBuffer(ReadOnlySpan<TDataType> data, CommandList list, GraphicsDevice device)
        {
            if (data.Length > Length)
            {
                BufferObject.Dispose();
                
                BufferDescription bufferDescription = new BufferDescription((uint) (data.Length * sizeof(TDataType)),
                    BufferUsage.UniformBuffer);
                BufferObject = device.ResourceFactory.CreateBuffer(bufferDescription);
                BufferObject.Name = _name;
            }
            list.UpdateBuffer(BufferObject, 0, data);
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
            return BufferObject;
        }
    }
}