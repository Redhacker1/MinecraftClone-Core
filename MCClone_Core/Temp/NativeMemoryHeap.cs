﻿using System;
using System.Runtime.InteropServices;

namespace MCClone_Core.Temp;

public sealed unsafe class NativeMemoryHeap : MemoryHeap
{
    public static NativeMemoryHeap Instance { get; } = new();

    private NativeMemoryHeap()
    {
    }

    public override nuint GetBlockSize(nuint byteCapacity)
    {
        return byteCapacity;
    }

    public override IntPtr Alloc(nuint byteCapacity, out nuint actualByteCapacity)
    {
        actualByteCapacity = byteCapacity;
        if (byteCapacity == 0)
        {
            return IntPtr.Zero;
        }

        GC.AddMemoryPressure((long)byteCapacity);
        return (IntPtr)NativeMemory.Alloc(byteCapacity);
    }

    public override void Free(nuint byteCapacity, IntPtr buffer)
    {
        NativeMemory.Free((void*)buffer);
        GC.RemoveMemoryPressure((long)byteCapacity);
    }

    public override IntPtr Realloc(
        IntPtr buffer,
        nuint previousByteCapacity,
        nuint requestedByteCapacity,
        out nuint actualByteCapacity)
    {
        if (previousByteCapacity == requestedByteCapacity)
        {
            actualByteCapacity = requestedByteCapacity;
            return buffer;
        }
            
        actualByteCapacity = requestedByteCapacity;
        if (requestedByteCapacity == 0)
        {
            Free(previousByteCapacity, buffer);
            return IntPtr.Zero;
        }

        GC.AddMemoryPressure((long)requestedByteCapacity);
        IntPtr newBuffer= (IntPtr)NativeMemory.Realloc((void*)buffer, requestedByteCapacity);

        GC.RemoveMemoryPressure((long)previousByteCapacity);
        return newBuffer;
    }
}