using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using Engine.Utilities.LowLevel.Memory;
using static System.Runtime.CompilerServices.Unsafe;

namespace MCClone_Core.Temp;

public unsafe class SafeByteStore<T> : CriticalFinalizerObject
        where T : unmanaged
{
    private T* _head;

    public MemoryHeap Heap { get; }
    public T* Buffer { get; private set; }
    public T* Head => _head;

    public uint ByteCapacity { get; private set; }
    public uint ByteCount => (uint)((byte*)_head - (byte*)Buffer);

    public uint Count => ByteCount / (uint)SizeOf<T>();
    public uint Capacity => ByteCapacity / (uint)SizeOf<T>();

    public Span<T> Span => new(Buffer, (int)Count);
    public Span<T> FullSpan => new(Buffer, (int)Capacity);

    public SafeByteStore(MemoryHeap heap, T* buffer, uint byteCapacity)
    {
        Heap = heap ?? throw new ArgumentNullException(nameof(heap));
        ByteCapacity = byteCapacity;
        Buffer = buffer;
        _head = Buffer;
    }

    public SafeByteStore(MemoryHeap heap) : this(heap, null, 0)
    {
    }

    public SafeByteStore(MemoryHeap heap, uint capacity) : this(heap, null, 0)
    {
        Resize(capacity);
    }

    /// <summary>
    /// Duplicates the contents of this <see cref="ByteStore{T}"/> by using a new heap.
    /// </summary>
    /// <param name="heap">The new heap to use.</param>
    /// <returns>A new <see cref="ByteStore{T}"/> using the specified <paramref name="heap"/>.</returns>
    public SafeByteStore<T> Clone(MemoryHeap heap)
    {
        uint byteCount = ByteCount;
        if (byteCount == 0)
        {
            return new SafeByteStore<T>(heap);
        }

        IntPtr newBuffer = heap.Alloc(byteCount, out nuint newByteCapacity);
        CopyBlockUnaligned((void*)newBuffer, Buffer, byteCount);

        SafeByteStore<T> newStore = new SafeByteStore<T>(heap, (T*) newBuffer, (uint)newByteCapacity);
        newStore._head = (T*)((byte*)newStore._head + byteCount);
        return newStore;
    }

    /// <summary>
    /// Duplicates the contents of this <see cref="ByteStore{T}"/>.
    /// </summary>
    /// <returns>A new <see cref="ByteStore{T}"/> using the current <see cref="Heap"/>.</returns>
    public SafeByteStore<T> Clone()
    {
        return Clone(Heap);
    }

    public void Trim()
    {
        Resize(Count);
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

    public static SafeByteStore<T> Create(MemoryHeap heap, uint capacity)
    {
        IntPtr buffer = heap.Alloc((nuint)(capacity * SizeOf<T>()), out nuint actualByteCapacity);
        return new SafeByteStore<T>(heap, (T*)buffer, (uint)actualByteCapacity);
    }

    private void Resize(uint newCapacity)
    {
        nuint byteCount = ByteCount;
        IntPtr newBuffer = Heap.Realloc(
            (IntPtr)Buffer, 
            ByteCapacity,
            newCapacity * (uint)SizeOf<T>(), 
            out nuint newByteCapacity);

        Buffer = (T*)newBuffer;
        _head = (T*)((byte*)Buffer + byteCount);
        ByteCapacity = (uint)newByteCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(uint capacity)
    {
        if (ByteCapacity < capacity * SizeOf<T>())
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

    ~SafeByteStore()
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