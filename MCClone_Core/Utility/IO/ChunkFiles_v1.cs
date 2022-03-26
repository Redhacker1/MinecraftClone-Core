#if !Core
using Godot;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using MCClone_Core.World_CS.Generation;

namespace MCClone_Core.Utility.IO
{
    public class ChunkFilesV1 : BaseFileHandler
    {

        
        public override ChunkCs GetChunkData(WorldData world, Int2 location, out bool chunkExists)
        {
            bool compressed = false;
            // TODO Come up with newer and shorter filename structure that will work when I batch chunks together
            string filename = GetFilename(new Int2((int)MathF.Floor(location.X), (int)MathF.Floor(location.Y)),world,false);
            string filePath = Path.Combine(world.Directory, filename);
            
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(world.Directory,GetFilename(new Int2((int)MathF.Floor(location.X), (int)MathF.Floor(location.Y)),world,true));
                if (!File.Exists(filePath))
                {
                    chunkExists = false;
                    return null;
                }
                compressed = true;
            }
            
            var FileData = File.ReadAllBytes(filePath);
            MemoryStream data = new MemoryStream(FileData);

            DeflateStream compressor = null;
            BinaryReader fileReader = new BinaryReader(data);
            if (compressed)
            {
                compressor = new DeflateStream(data, CompressionMode.Decompress);
                fileReader = new BinaryReader(compressor);
            }


            SaveInfo saveData = new SaveInfo();
            
            saveData.VersionNumber = fileReader.ReadByte();
            
            saveData.BlockSize = fileReader.ReadByte();
            saveData.BiomeId = fileReader.ReadByte();
            
            short blockIds = fileReader.ReadInt16();

            Dictionary<byte, byte> blockDict = new Dictionary<byte, byte>();
            for (int i = 0; i < blockIds; i++)
            {
                // Block ID
                byte bid = fileReader.ReadByte();
                
                // Serialized ID
                byte sid = fileReader.ReadByte();
                
                blockDict.Add(sid, bid);
            }

            int x = fileReader.ReadInt32();
            int y = fileReader.ReadInt32();

            saveData.Location = new Int2(x, y);
            
            // TODO: Add chunk Maxs to file format and have it calculate this value automatically
            byte[] serializedBlockData = fileReader.ReadBytes(ChunkCs.MaxX * ChunkCs.MaxY * ChunkCs.MaxZ);
            saveData.ChunkBlocks = new byte[ChunkCs.MaxX * ChunkCs.MaxY * ChunkCs.MaxZ];

            for (int i = 0; i <  serializedBlockData.Length; i++)
            {
                saveData.ChunkBlocks[i] = blockDict[serializedBlockData[i]];
            }

            if (compressed)
            {
                compressor.Close();    
            }
            
            data.Close();

            ChunkCs referencedChunk = new ChunkCs
            {
                BlockData = saveData.ChunkBlocks,
                ChunkCoordinate = new Int2(saveData.Location.X, saveData.Location.Y),
                Pos = new Vector3(ChunkCs.MaxX * saveData.Location.X, 0, ChunkCs.MaxX * saveData.Location.Y)

            };
            


            chunkExists = true;
            return referencedChunk;
        }

        public override void WriteChunkData(byte[] blocks, Int2 chunkCoords, WorldData world,
            bool optimizeSave = true)
        {
            SaveInfo saveData = SerializeChunkData(blocks,chunkCoords, world, optimizeSave);
            
            MemoryStream chunkdat = new MemoryStream();
            
            BinaryWriter fileWriter = new BinaryWriter(chunkdat);

            fileWriter.Write(saveData.VersionNumber);
            fileWriter.Write(saveData.BlockSize);
            fileWriter.Write(saveData.BiomeId);

            fileWriter.Write((short)saveData.BlockPalette.Count);
            foreach (KeyValuePair<byte, byte> blockIdPair in saveData.BlockPalette)
            {
                fileWriter.Write(blockIdPair.Key);
                fileWriter.Write(blockIdPair.Value);
            }
            
            
            fileWriter.Write(saveData.Location.X);
            fileWriter.Write(saveData.Location.Y);
            Span<byte> encodedBytes = stackalloc byte[saveData.ChunkBlocks.Length];

            for (int blockIndex = 0; blockIndex < encodedBytes.Length; blockIndex++)
            {
                encodedBytes[blockIndex] = saveData.BlockPalette[saveData.ChunkBlocks[blockIndex]];
            }
            fileWriter.Write(encodedBytes);

            if(world.Directory == null) return;
            FileStream fs = new FileStream(Path.Combine(world.Directory, GetFilename(new Int2((int)MathF.Floor(chunkCoords.X), (int)MathF.Floor(chunkCoords.Y)), world, optimizeSave)), FileMode.Create);

            
            if (optimizeSave)
            {

                string uncompressedPath = Path.Combine(world.Directory, $"{world.Name}_x_{saveData.Location.X}-y_{saveData.Location.Y}.cdat");
                if (File.Exists(uncompressedPath))
                {
                    File.Delete(uncompressedPath);
                }

                byte[] cdat = Compress(chunkdat.GetBuffer());
                fs.Write(cdat, 0, cdat.Length);
            }
            else
            {
                fs.Write(chunkdat.ToArray(), 0, (int)chunkdat.Length);
            }
            
            fileWriter.Close();
            chunkdat.Close();
            fs.Close();


        }

        public static byte[] Compress(byte[] data)
        {

            byte[] compressArray = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(data, 0, data.Length);
                }
                compressArray = memoryStream.ToArray();
            }
            return compressArray;
        }

    }
}