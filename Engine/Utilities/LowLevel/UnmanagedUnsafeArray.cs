using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Utilities.LowLevel;

public unsafe struct UnmanagedUnsafeArray<T> : IDisposable where T : unmanaged
{

    public bool IsEmpty => _start == null;
    public uint Length;
    T* _start = null;


    public UnmanagedUnsafeArray(uint length) : this((T*)NativeMemory.Alloc((UIntPtr) (length * Unsafe.SizeOf<T>())), length)
    {
    }
    
    UnmanagedUnsafeArray(T* ptr, uint length)
    {
        Length = length;
        _start = ptr;
    }


    public UnmanagedUnsafeArray<TCastTo> As<TCastTo>() where TCastTo : unmanaged
    {
        return new UnmanagedUnsafeArray<TCastTo>((TCastTo*)_start, Length * ((uint)Unsafe.SizeOf<T>() / (uint)Unsafe.SizeOf<TCastTo>()));
    }

    public void Resize(uint newCapacity)
    {
        Length = newCapacity;
        _start = (T*)NativeMemory.Realloc(_start, (UIntPtr)(newCapacity * (uint) Unsafe.SizeOf<T>()));
    }


    T* SafeAccess(uint index)
    {
        if (index > Length)
        {
            return &_start[index];
        }
        throw new IndexOutOfRangeException();
    }



    public T this[uint index]
    {
        get => *SafeAccess(index);
        set => *SafeAccess(index) = value;
    }

    public T* GetPtrAt(uint index)
    {
        return SafeAccess(index);
    }


    public void Dispose()
    {
        NativeMemory.Free(_start);
    }
}