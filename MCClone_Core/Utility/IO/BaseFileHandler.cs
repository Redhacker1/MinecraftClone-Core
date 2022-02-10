using System.Collections.Generic;

using MCClone_Core.World_CS.Generation;
using File = System.IO.File;
using Path = System.IO.Path;
using Vector2 = System.Numerics.Vector2;

namespace MCClone_Core.Utility.IO
{
    public abstract class BaseFileHandler
    {
        public byte VersionNumber = 1;

        public string FileExtension = ".cdat";

        public abstract ChunkCs GetChunkData(WorldData world, Vector2 location, out bool chunkExists);
        public abstract void WriteChunkData(byte[] blocks, Vector2 chunkCoords, WorldData world,
            bool optimizeSave = true);

        public virtual string GetFilename(Vector2 chunkCoords, WorldData world, bool compressed)
        {
            if (compressed)
            {
                return $"{world.Name}_x_{(int) chunkCoords.X}-y_{(int) chunkCoords.Y}.{FileExtension}_c";
            }
            return $"{world.Name}_x_{(int)chunkCoords.X}-y_{(int)chunkCoords.Y}.{FileExtension}";

        }


        public virtual bool ChunkExists(WorldData world, Vector2 location)
        {
            string filename = GetFilename(location, world, false);
            if (world.Directory == null) return false;
            string filePath = Path.Combine(world.Directory, filename);

            if (File.Exists(filePath))
            {
                return true;
            }
            
            filename = GetFilename(location, world, true);
            filePath = Path.Combine(world.Directory, filename);
            return File.Exists(filePath);
        }
        
        
        protected virtual SaveInfo SerializeChunkData(byte[] blocks, Vector2 chunkCoords, WorldData world,
            bool optimizeStorage)
        {
            SaveInfo chunkSaveData = new SaveInfo();
            chunkSaveData.BlockSize = 1; // Default the block size to 1, I will determine if further optimizations are needed in the next step
            chunkSaveData.VersionNumber = 1; // Again Default Version number to one
            chunkSaveData.Location = chunkCoords; // Location of the chunk, will be saved somehow in chunk file
            chunkSaveData.BiomeId = 0; // Currently Unused Will come eventually
            chunkSaveData.World = world.Directory + world.Name;
            chunkSaveData.ChunkBlocks = blocks;
            

            List<byte> Blocks = new List<byte>(128);
            for (int i = 0; i < blocks.Length; i++)
            {
                if (Blocks.Contains(blocks[i]) == false)
                {
                    Blocks.Add(blocks[i]);
                }
            }

            //IEnumerable<byte> blockTypes = distinctBlocks as byte[] ?? distinctBlocks.ToArray();
            if (optimizeStorage && Blocks.Count < 16)
            {
                chunkSaveData.VersionNumber = 0; // Tells the save file writer to compress the blocks into two per byte (or later as small as it can get it)
            }


            Dictionary<byte, byte> blockDict = new Dictionary<byte, byte>();    
            byte serializedId = 0;
            foreach (byte blockType in Blocks)
            {
                blockDict[blockType] = serializedId;
                serializedId+=1;
            }

            chunkSaveData.BlockIdWriter = blockDict;
            return chunkSaveData;

        }
    }
}