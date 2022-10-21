using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace Engine.Rendering.Veldrid
{

    public abstract class BaseStructuredBuffer
    {
        public abstract DeviceBuffer GetBuffer();
    }
    public unsafe class StructuredBuffer<TDataType> : BaseUnifomrBuffer where TDataType : unmanaged
    {
        private readonly uint _alignment;

        internal DeviceBuffer bufferObject;
        GraphicsDevice _device;
        readonly bool CanBeWritten;

        public StructuredBuffer(GraphicsDevice gDevice, uint length, uint alignment, bool writable)
        {
            _device = gDevice;
            _alignment = unchecked((alignment + (16u - 1u)) & (uint)(-16));
            CanBeWritten = writable;
            Resize(length);
        }

        public StructuredBuffer(GraphicsDevice gDevice, uint length, bool writable = false) : this(gDevice, length, (uint)Unsafe.SizeOf<TDataType>(), writable)
        {
        }

        public uint ByteLength => bufferObject?.SizeInBytes ?? 0;
        public uint Length => bufferObject?.SizeInBytes / _alignment ?? 0;

        public void Dispose()
        {
            bufferObject.Dispose();
        }

        public void Resize(uint capacity)
        {
            bufferObject?.Dispose();

            bufferObject = _device.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    capacity * _alignment,
                    CanBeWritten ? BufferUsage.StructuredBufferReadWrite : BufferUsage.StructuredBufferReadOnly | BufferUsage.StructuredBufferReadOnly,
                    (uint)Unsafe.SizeOf<TDataType>(), true)
                );
        }

        public void ModifyBuffer(ReadOnlySpan<byte> data, uint alignment, CommandList? list = null)
        {
            uint count = (uint)data.Length / alignment;
            Debug.Assert(data.Length % alignment == 0);

            uint capacity = bufferObject.SizeInBytes / _alignment;

            if (count > capacity || bufferObject.IsDisposed)
            {
                Resize(count);
            }

            if (alignment == _alignment)
            {


                if (list != null)
                    list.UpdateBuffer(bufferObject, 0, data);
                else
                    _device.UpdateBuffer(bufferObject, 0, data);
            }
            else
            {


                uint alignmentAmount = alignment / _alignment;
                alignmentAmount += alignment % _alignment;

                if (data.Length >= capacity)
                {
                    Resize(count * alignmentAmount * _alignment);
                }
                
                if (list != null)
                    list.UpdateBuffer(bufferObject, 0, data);
                else
                    _device.UpdateBuffer(bufferObject, 0, data);
            }
        }

        public void ModifyBuffer(ReadOnlySpan<TDataType> data, uint alignment, CommandList list = null)
        {
            ModifyBuffer(MemoryMarshal.AsBytes(data), alignment, list);
        }

        public void ModifyBuffer(ReadOnlySpan<TDataType> data, CommandList list = null)
        {
            ModifyBuffer(data, (uint)Unsafe.SizeOf<TDataType>(), list);
        }

        public void ModifyBuffer(TDataType data, CommandList list = null)
        {
            ModifyBuffer(MemoryMarshal.CreateSpan(ref data, 1), list);
        }

        public override DeviceBuffer GetBuffer()
        {
            return bufferObject;
        }
    }
}