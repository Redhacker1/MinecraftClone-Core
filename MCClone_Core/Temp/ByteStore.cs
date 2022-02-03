using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MCClone_Core.Temp
{
    public unsafe class ByteStore<T>
        where T : unmanaged
    {
        private T* _head;
        
        public T* Buffer { get; private set; }
        public T* Head => _head;

        public uint ByteCapacity { get; private set; }
        public uint ByteCount => (uint)((byte*)_head - (byte*)Buffer);

        public uint Count => ByteCount / (uint)Unsafe.SizeOf<T>();
        public uint Capacity => ByteCapacity / (uint)Unsafe.SizeOf<T>();

        public unsafe Span<T> Span => new(Buffer, (int)Count);
        public unsafe Span<T> FullSpan => new(Buffer, (int)Capacity);

        public ByteStore(T* buffer, uint byteCapacity)
        {
            ByteCapacity = byteCapacity;
            Buffer = buffer;
            _head = Buffer;
            
        }

        public ByteStore()
        {
            ByteCapacity = (uint) Unsafe.SizeOf<T>();
            Buffer = (T*) NativeMemory.Alloc((nuint) Unsafe.SizeOf<T>());
            _head = Buffer;
        }

        public ByteStore(uint capacity) : this(null, 0)
        {
            Resize(capacity);
        }

        public ByteStore<T> Clone(ref ByteStore<T> Destination)
        {
            uint byteCount = ByteCount;
            if (byteCount == 0)
                return new ByteStore<T>();

            IntPtr newBuffer = (IntPtr) NativeMemory.Alloc(byteCount);
            Unsafe.CopyBlockUnaligned((void*)newBuffer, Buffer, byteCount);

            ByteStore<T> newStore = new((T*)newBuffer, byteCount);
            newStore._head = (T*)((byte*)newStore._head + byteCount);
            return newStore;
        }

        public void MoveByteHead(uint byteCount)
        {
            Debug.Assert((byte*)_head + byteCount <= (byte*)Buffer + ByteCapacity);
            _head = (T*)((byte*)_head + byteCount);
        }

        public void MoveHead(uint count)
        {
            Debug.Assert(_head + count <= Buffer + Capacity);
            _head += count;
        }

        public static ByteStore<T> Create(uint capacity)
        {
            IntPtr buffer = (IntPtr) NativeMemory.AllocZeroed(capacity * (uint)Unsafe.SizeOf<T>());
            return new ByteStore<T>((T*)buffer, capacity * (uint)Unsafe.SizeOf<T>());
        }

        private unsafe void Resize(uint newCapacity)
        {
            uint byteCount = ByteCount;
            IntPtr newBuffer = (IntPtr) NativeMemory.Realloc(Buffer, newCapacity * (uint)Unsafe.SizeOf<T>());

            Buffer = (T*)newBuffer;
            _head = (T*)((byte*)Buffer + byteCount);
            ByteCapacity = newCapacity * (uint)Unsafe.SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint capacity)
        {

            if (ByteCapacity < capacity * Unsafe.SizeOf<T>())
            {
                uint newCapacity = Math.Min(capacity * 2, capacity + 1024 * 64);
                Resize(newCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepareCapacityFor(uint count)
        {
            EnsureCapacity(Count + count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T value)
        {
            *_head++ = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetAppendSpan(uint count)
        {
            Span<T> slice = new(_head, (int)count);
            _head += count;
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendRange(ReadOnlySpan<T> values)
        {
            values.CopyTo(new(_head, values.Length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetAppendPtr(uint count)
        {
            T* r = _head;
            _head += count;
            return r;
        }

        public void Clear()
        {
            _head = Buffer;
        }

        public void Dispose()
        {
            IntPtr buffer = (IntPtr)Buffer;
            if (buffer != IntPtr.Zero)
                NativeMemory.Free((void*) buffer);
            Buffer = null;
            _head = null;
        }
    }
}