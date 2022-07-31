using System;
using System.Collections.Generic;

namespace MCClone_Core.Temp
{
    // TODO: Not shamelessly rip from people
    // Shamelessly ripped from: https://github.com/TechnologicalPizza/VoxelPizza/tree/0faa11d474d861bc60d6dc523f66deb4688be709
    // Author: Technopizza
    public partial class HeapPool
    {
        public unsafe class Segment
        {
            private Stack<IntPtr> _pooled = new();

            public nuint BlockSize { get; }
            public uint MaxCount { get; }

            public uint Count => (uint)_pooled.Count;

            public Segment(nuint blockSize, uint maxCount)
            {
                BlockSize = blockSize;
                MaxCount = maxCount;
            }

            public IntPtr Rent(MemoryHeap heap, out nuint actualByteCapacity)
            {
                lock (_pooled)
                {
                    if (_pooled.TryPop(out IntPtr pooled))
                    {
                        actualByteCapacity = BlockSize;
                        return pooled;
                    }
                }

                return heap.Alloc(BlockSize, out actualByteCapacity);
            }

            public void Return(MemoryHeap heap, IntPtr buffer)
            {
                lock (_pooled)
                {
                    if ((uint)_pooled.Count < MaxCount)
                    {
                        _pooled.Push((IntPtr)buffer);
                        return;
                    }
                }

                heap.Free(BlockSize, buffer);
            }
        }
    }
}