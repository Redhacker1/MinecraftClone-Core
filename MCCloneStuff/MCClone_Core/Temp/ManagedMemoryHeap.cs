using System;
using System.Collections.Concurrent;

namespace MCClone_Core.Temp;

public class ManagedMemoryHeap : MemoryHeap
{
    
    ConcurrentDictionary<IntPtr, byte[]> Blocks = new ConcurrentDictionary<IntPtr, byte[]>();
    public static ManagedMemoryHeap Instance { get; } = new();
    public override nuint GetBlockSize(nuint byteCapacity)
    {
        return byteCapacity;
    }

    public override unsafe IntPtr Alloc(nuint byteCapacity, out nuint actualByteCapacity)
    {
        byte[] array = GC.AllocateArray<byte>((int) byteCapacity, true);
        actualByteCapacity = (nuint) array.Length;
        IntPtr address;
        
        fixed (byte* arrayaddress = array)
        {
            address = (IntPtr)arrayaddress;   
        }

        if (Blocks.TryAdd(address, array) == false)
        {
            throw new InvalidOperationException("This memory address has already been accquired, please report to Donovan Strawhacker");
        }

        return address;
    }

    public override unsafe void Free(nuint byteCapacity, IntPtr buffer)
    {
        if (Blocks.ContainsKey(buffer))
        {
            Blocks.TryRemove(buffer, out _);
        } 
    }
}