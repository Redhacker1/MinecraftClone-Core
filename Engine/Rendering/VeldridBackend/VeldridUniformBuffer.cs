using System.Numerics;
using Engine.Initialization;
using Engine.Rendering.Shared.Pipeline.Resources;
using Engine.Rendering.Shared.Shaders;
using Veldrid;

namespace Engine.Rendering.VeldridBackend
{
    class VeldridUniformBuffer<TDataType> : UniformBuffer<TDataType> where TDataType : unmanaged 
    {
        internal DeviceBuffer UnderlyingBuffer;
        protected VeldridUniformBuffer(string name, ShaderType stage, uint count, GraphicsDevice device) : base(name, stage, count)
        {
            UnderlyingBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription(count, BufferUsage.UniformBuffer));
        }
        
        protected VeldridUniformBuffer(string name, ShaderType stage, TDataType[] data, GraphicsDevice device) : base(name, stage, data)
        {
            unsafe
            {
                UnderlyingBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(data.Length * sizeof(TDataType)), BufferUsage.UniformBuffer));
            }
            
            device.UpdateBuffer(UnderlyingBuffer, 0, data);
        }

    }
}