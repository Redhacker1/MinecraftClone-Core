using System;
using System.Runtime.ConstrainedExecution;
using Engine.Utilities.LowLevel.Memory;

namespace MCClone_Core.Temp;

public unsafe class SafeByteStore<T> : CriticalFinalizerObject, IDisposable
        where T : unmanaged
{
    internal ByteStore<T> BackingStore;
    public Span<T> Span => BackingStore.Span;

    public Span<T> FullSpan => BackingStore.FullSpan;
    public uint Capacity => (uint)BackingStore.Capacity;
    public uint Count => BackingStore.Count;

    public static SafeByteStore<T> Create(MemoryHeap heap, uint capacity)
    {
        return new SafeByteStore<T>()
        {
            BackingStore = ByteStore<T>.Create(heap, capacity)
        };
    }

    ~SafeByteStore()
    {
        ReleaseUnmanagedResources();
    }

    void ReleaseUnmanagedResources()
    {
        BackingStore.Dispose();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public void Clear()
    {
        BackingStore.Clear();
    }

    public void EnsureCapacity(uint capacity)
    {
        BackingStore.EnsureCapacity(capacity);
    }

    public void PrepareCapacityFor(uint count)
    {
        BackingStore.PrepareCapacityFor(count);
    }

    public void Append(T value)
    {
        BackingStore.Append(value);
    }

    public void AppendRange(ReadOnlySpan<T> values)
    {
        BackingStore.AppendRange(values);
    }

    public Span<T> GetAppendSpan(uint count)
    {
        return BackingStore.GetAppendSpan(count);
    }

    public void Trim()
    {
        BackingStore.Trim();
    }
}