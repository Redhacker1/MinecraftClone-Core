using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.Unsafe;

namespace MCClone_Core.Temp
{
    // TODO: Not shamelessly rip from people
    // Shamelessly ripped from: https://github.com/TechnologicalPizza/VoxelPizza/tree/0faa11d474d861bc60d6dc523f66deb4688be709
    // Author: Technopizza
    public unsafe class ByteStore<T>
        where T : unmanaged
    {
        private T* _head;

        public MemoryHeap Heap { get; }
        public T* Buffer { get; private set; }
        public T* Head => _head;

        public nuint ByteCapacity { get; private set; }
        public nuint ByteCount => (nuint)((byte*)_head - (byte*)Buffer);

        public nuint Count => ByteCount / (nuint)SizeOf<T>();
        public nuint Capacity => ByteCapacity / (nuint)SizeOf<T>();

        public Span<T> Span => new(Buffer, (int)Count);
        public Span<T> FullSpan => new(Buffer, (int)Capacity);

        public ByteStore(MemoryHeap heap, T* buffer, nuint byteCapacity)
        {
            Heap = heap ?? throw new ArgumentNullException(nameof(heap));
            ByteCapacity = byteCapacity;
            Buffer = buffer;
            _head = Buffer;
        }

        public ByteStore(MemoryHeap heap) : this(heap, null, 0)
        {
        }

        public ByteStore(MemoryHeap heap, nuint capacity) : this(heap, null, 0)
        {
            Resize(capacity);
        }

        /// <summary>
        /// Duplicates the contents of this <see cref="ByteStore{T}"/> by using a new heap.
        /// </summary>
        /// <param name="heap">The new heap to use.</param>
        /// <returns>A new <see cref="ByteStore{T}"/> using the specified <paramref name="heap"/>.</returns>
        public ByteStore<T> Clone(MemoryHeap heap)
        {
            nuint byteCount = ByteCount;
            if (byteCount == 0)
            {
                return new ByteStore<T>(heap);
            }

            IntPtr newBuffer = heap.Alloc(byteCount, out nuint newByteCapacity);
            CopyBlockUnaligned((void*)newBuffer, Buffer, (uint)byteCount);

            ByteStore<T> newStore = new(heap, (T*)newBuffer, newByteCapacity);
            newStore._head = (T*)((byte*)newStore._head + byteCount);
            return newStore;
        }

        /// <summary>
        /// Duplicates the contents of this <see cref="ByteStore{T}"/>.
        /// </summary>
        /// <returns>A new <see cref="ByteStore{T}"/> using the current <see cref="Heap"/>.</returns>
        public ByteStore<T> Clone()
        {
            return Clone(Heap);
        }

        public void Trim()
        {
            Resize(Count);
        }

        public void MoveByteHead(nuint byteCount)
        {
            Debug.Assert((byte*)_head + byteCount <= (byte*)Buffer + ByteCapacity);
            _head = (T*)((byte*)_head + byteCount);
        }

        public void MoveHead(uint count)
        {
            Debug.Assert(_head + count <= Buffer + Capacity);
            _head += count;
        }

        public static ByteStore<T> Create(MemoryHeap heap, nuint capacity)
        {
            IntPtr buffer = heap.Alloc(capacity * (nuint)SizeOf<T>(), out nuint actualByteCapacity);
            return new ByteStore<T>(heap, (T*)buffer, actualByteCapacity);
        }

        private void Resize(nuint newCapacity)
        {
            nuint byteCount = ByteCount;
            IntPtr newBuffer = Heap.Realloc(
                (IntPtr)Buffer, 
                ByteCapacity,
                newCapacity * (uint)SizeOf<T>(), 
                out nuint newByteCapacity);

            Buffer = (T*)newBuffer;
            _head = (T*)((byte*)Buffer + byteCount);
            ByteCapacity = newByteCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(nuint capacity)
        {
            if (ByteCapacity < capacity * (nuint)SizeOf<T>())
            {
                nuint newCapacity = Math.Min(capacity * 2, capacity + 1024 * 64);
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
            _head += values.Length;
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
            void* buffer = Buffer;
            Buffer = null;
            if (buffer != null)
            {
                Heap.Free(ByteCapacity, (IntPtr)buffer);
            }
            _head = null;
        }
    }
}