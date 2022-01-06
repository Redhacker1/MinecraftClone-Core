using System;
using System.IO;
using System.Runtime.CompilerServices;
using Engine.Rendering.Shared;
using Engine.Rendering.Shared.Buffers;
using Veldrid;

namespace Engine.Rendering.VeldridBackend
{
    sealed class VeldridBuffer<T> : BaseBufferObject<T> where T : unmanaged
    {
        internal readonly DeviceBuffer UnderlyingBuffer;

        public VeldridBuffer(T[] data, BufferType bufferType, GraphicsDevice device) : base(data, bufferType)
        {
            if (BufferType.IndexBuffer == bufferType && new T() is uint == false && new T() is ushort == false)
            {
                throw new ArgumentException($"Can not use type of {typeof(T)} in index buffer");
            }
            
            unsafe
            {
                BufferDescription description = new BufferDescription()
                {
                    SizeInBytes = (uint)(sizeof(T) * data.Length + 1),
                    Usage = GetUsage(bufferType)
                };
                UnderlyingBuffer = device.ResourceFactory.CreateBuffer(description);
                device.UpdateBuffer(UnderlyingBuffer, 0, data);
            }
        }

        static BufferUsage GetUsage(BufferType type)
        {
            return type switch
            {
                BufferType.IndexBuffer => BufferUsage.IndexBuffer,
                BufferType.VertexBuffer => BufferUsage.VertexBuffer,
                _ => throw new InvalidOperationException("Not a valid return type")
            };
        }
        
        
    }
}