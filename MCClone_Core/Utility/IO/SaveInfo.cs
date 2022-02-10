using System.Collections.Generic;
using System.Numerics;
using MCClone_Core.World_CS.Generation;

namespace MCClone_Core.Utility.IO
{
    public struct SaveInfo
    {
        public byte BlockSize;
        public byte VersionNumber;
        public Vector2 Location;
        public string World;
        public byte BiomeId;
        public byte[] ChunkBlocks;
        public Dictionary<byte, byte> BlockIdWriter;
    }
}