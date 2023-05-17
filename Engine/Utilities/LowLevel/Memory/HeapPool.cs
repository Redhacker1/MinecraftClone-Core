using System;
using System.Threading;

namespace Engine.Utilities.LowLevel.Memory
{
    // TODO: Not shamelessly rip from people
    // Shamelessly ripped from: https://github.com/TechnologicalPizza/VoxelPizza/tree/0faa11d474d861bc60d6dc523f66deb4688be709
    // Author: Technopizza
    public unsafe partial class HeapPool : MemoryHeap
    {
        private Segment[] _segments;

        // Capacities between 1 and 2^MinRangeBits bytes fit in the first segment.
        private const int MinRangeBits = 8;

        private const uint MinMask = ~0u >> (32 - MinRangeBits);

        private const nuint StepSize = 1024;
        
        

        public ulong AvailableBytes
        {
            get
            {
                ulong total = 0;
                foreach (Segment segment in _segments)
                {
                    total += segment.BlockSize * segment.Count;
                }
                return total;
            }
        }

        public ulong BytesInUse;

        public MemoryHeap Heap { get; }
        public uint MaxCapacity { get; }

        public HeapPool(MemoryHeap heap, uint maxCapacity)
        {
            Heap = heap ?? throw new ArgumentNullException(nameof(heap));
            MaxCapacity = maxCapacity;

            _segments = new Segment[GetSegmentIndex(MaxCapacity)];
            for (int i = 0; i < _segments.Length; i++)
            {
                nuint blockSize = GetBlockSizeAt((nuint)i);
                //uint maxCount = Math.Max(4u, 1024u >> Math.Max(0, i - 6));
                uint maxCount = Math.Max(1024 / (uint)(i + 1), 4);

                _segments[i] = new Segment(blockSize, maxCount);
            }
        }

        private static nuint GetSegmentIndex(nuint byteCapacity)
        {
            //int poolIndex = BitOperations.Log2(byteCapacity - 1 | MinMask) - (MinRangeBits - 1);
            nuint poolIndex = (byteCapacity - 1) / StepSize;
            return poolIndex;
        }

        private static nuint GetBlockSizeAt(nuint index)
        {
            return (index + 1) * StepSize;
            //return 1u << ((int)index + MinRangeBits);
        }

        public override nuint GetBlockSize(nuint byteCapacity)
        {
            nuint i = GetSegmentIndex(byteCapacity);
            if (i >= (nuint)_segments.Length)
            {
                return Heap.GetBlockSize(byteCapacity);
            }
            return GetBlockSizeAt(i);
        }

        public Segment? GetSegment(nuint byteCapacity)
        {
            nuint index = GetSegmentIndex(byteCapacity);
            Segment[] segments = _segments;
            if (index >= (nuint)segments.Length)
            {
                return null;
            }
            return segments[index];
        }

        public override IntPtr Alloc(nuint byteCapacity, out nuint actualByteCapacity)
        {
            Segment? segment = GetSegment(byteCapacity);
            IntPtr allocAddress;
            if (segment.HasValue != true)
            {
                allocAddress = Heap.Alloc(byteCapacity, out actualByteCapacity);
            }
            else
            {
                allocAddress = segment.Value.Rent(Heap, out actualByteCapacity);    
            }
            AddToByteCount(actualByteCapacity);
            return allocAddress;


        }

        public void AddToByteCount(ulong additional)
        {
            Interlocked.Add(ref BytesInUse, additional);
        }

        public override void Free(nuint byteCapacity, IntPtr buffer)
        {
            Segment? segment = GetSegment(byteCapacity);
            AddToByteCount(0-byteCapacity);
            if (segment.HasValue && segment.Value.BlockSize == byteCapacity)
            {
                segment.Value.Return(Heap, buffer);
                return;
            }
            Heap.Free(byteCapacity, buffer);
        }

        public override IntPtr Realloc(
            IntPtr buffer,
            nuint previousByteCapacity,
            nuint requestedByteCapacity,
            out nuint actualByteCapacity)
        {

            IntPtr endLocation;
            
            if (requestedByteCapacity > MaxCapacity)
            {
                endLocation = Heap.Realloc(
                    buffer,
                    previousByteCapacity,
                    requestedByteCapacity,
                    out actualByteCapacity);
            }
            else
            {
                endLocation = base.Realloc(
                    buffer,
                    previousByteCapacity,
                    requestedByteCapacity,
                    out actualByteCapacity);
            }

            AddToByteCount(0-previousByteCapacity);
            AddToByteCount(actualByteCapacity);
            return endLocation;
        }
    }
}