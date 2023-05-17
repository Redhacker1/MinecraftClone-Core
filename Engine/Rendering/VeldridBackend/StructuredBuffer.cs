using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace Engine.Rendering.VeldridBackend
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
            _device.DisposeWhenIdle(bufferObject);
        }

        public void Resize(uint capacity)
        {
            if (bufferObject != null)
            {
                _device.DisposeWhenIdle(bufferObject);   
            }

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

            uint capacity = bufferObject.SizeInBytes / _alignment;

            if (count > capacity)
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
                Span<byte> buffer = stackalloc byte[2048];
                uint maxLength = (uint)(buffer.Length + _alignment - 1) / _alignment;

                if (count > capacity)
                {
                    Resize(count);
                }

                uint srcOffset = 0;
                uint dstOffset = 0;
                uint dstAlignment = _alignment;

                while (count > 0)
                {
                    uint toUpload = count;
                    if (toUpload > maxLength)
                        toUpload = maxLength;

                    uint srcLength = toUpload * alignment;
                    uint dstLength = toUpload * dstAlignment;

                    ReadOnlySpan<byte> src = data.Slice((int)srcOffset, (int)srcLength);
                    Span<byte> dst = buffer.Slice(0, (int)dstLength);

                    for (uint i = 0; i < toUpload; i++)
                    {
                        src.Slice((int)(i * alignment), (int)alignment).CopyTo(dst.Slice((int)(i * dstAlignment), (int)dstAlignment));
                    }

                    if (list != null)
                        list.UpdateBuffer(bufferObject, dstOffset, dst);
                    else
                        _device.UpdateBuffer(bufferObject, dstOffset, dst);

                    count -= toUpload;
                    srcOffset += srcLength;
                    dstOffset += dstLength;
                }
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