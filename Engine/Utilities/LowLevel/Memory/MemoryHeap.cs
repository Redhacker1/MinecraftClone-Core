using System;
using System.Runtime.CompilerServices;

namespace Engine.Utilities.LowLevel.Memory
{
    public abstract unsafe class MemoryHeap
    {
        public abstract nuint GetBlockSize(nuint byteCapacity);

        public abstract IntPtr Alloc(nuint byteCapacity, out nuint actualByteCapacity);

        public abstract void Free(nuint byteCapacity, IntPtr buffer);

        public virtual IntPtr Realloc(
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

            IntPtr newBuffer = Alloc(requestedByteCapacity, out actualByteCapacity);
            if (buffer != null)
            {
                Unsafe.CopyBlockUnaligned(
                    (void*)newBuffer,
                    (void*)buffer,
                    (uint)Math.Min(requestedByteCapacity, previousByteCapacity));

                Free(previousByteCapacity, buffer);
            }
            return newBuffer;
        }
    }   
}