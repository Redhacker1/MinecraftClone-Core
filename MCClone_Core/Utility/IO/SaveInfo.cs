using System;
using System.Collections.Generic;
using MCClone_Core.World_CS.Generation;

namespace MCClone_Core.Utility.IO
{
    public struct SaveInfo
    {
        public byte BlockSize;
        public byte VersionNumber;
        public Int2 Location;
        public byte BiomeId;
        public Memory<byte> ChunkBlocks;
        public Dictionary<byte, byte> BlockPalette;
    }
}