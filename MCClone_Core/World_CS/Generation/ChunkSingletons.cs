using MCClone_Core.Temp;
using MCClone_Core.World_CS.Generation.Chunk_Generator_cs;

namespace MCClone_Core.World_CS.Generation;

public static class ChunkSingletons
{
    public static readonly HeapPool ChunkPool = new HeapPool(NativeMemoryHeap.Instance, uint.MaxValue);
    public static readonly BaseGenerator Generator = new ForestGenerator();
}