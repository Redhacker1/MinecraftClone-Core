using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using Engine.Utilities.MathLib;
using MCClone_Core.World_CS.Generation;

namespace MCClone_Core.Utility.IO
{
    public class ChunkFilesV1 : BaseFileHandler
    {

        
        public override ChunkCs GetChunkData(WorldData world, Int2 location, out bool chunkExists)
        {
            bool compressed = false;
            // TODO Come up with newer and shorter filename structure that will work when I batch chunks together
            string filename = GetFilename(location, world, false);
            string filePath = Path.Combine(world.Directory, filename);
            
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(world.Directory,GetFilename(location, world, true));
                if (!File.Exists(filePath))
                {
                    chunkExists = false;
                    return null;
                }
                compressed = true;
            }
            
            byte[] fileData = File.ReadAllBytes(filePath);
            MemoryStream data = new MemoryStream(fileData);

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
            
            byte[] serializedBlockData = fileReader.ReadBytes(ChunkCs.MaxX * ChunkCs.MaxY * ChunkCs.MaxZ);
            ChunkCs referencedChunk = new ChunkCs
            {
                ChunkCoordinate = saveData.Location,
                Position = new Vector3(ChunkCs.MaxX * saveData.Location.X, 0, ChunkCs.MaxZ * saveData.Location.Y)
            };

            for (int i = 0; i <  serializedBlockData.Length; i++)
            {
                referencedChunk.BlockData.FullSpan[i] = blockDict[serializedBlockData[i]];
            }

            if (compressed)
            {
                compressor.Close();    
            }
            
            data.Close();

            chunkExists = true;
            return referencedChunk;
        }

        public override void WriteChunkData(Span<byte> blocks, Int2 chunkCoords, WorldData world,
            bool optimizeSave = true)
        {
            SaveInfo saveData = SerializeChunkData(blocks,chunkCoords, world, optimizeSave);
            
            MemoryStream outputData = new MemoryStream();
            
            BinaryWriter binaryWriter = new BinaryWriter(outputData);

            binaryWriter.Write(saveData.VersionNumber);
            binaryWriter.Write(saveData.BlockSize);
            binaryWriter.Write(saveData.BiomeId);

            binaryWriter.Write((short)saveData.BlockPalette.Count);
            foreach (KeyValuePair<byte, byte> blockIdPair in saveData.BlockPalette)
            {
                binaryWriter.Write(blockIdPair.Key);
                binaryWriter.Write(blockIdPair.Value);
            }
            
            
            binaryWriter.Write(saveData.Location.X);
            binaryWriter.Write(saveData.Location.Y);
            Span<byte> encodedBytes = new byte[saveData.ChunkBlocks.Length];

            for (int blockIndex = 0; blockIndex < encodedBytes.Length; blockIndex++)
            {
                encodedBytes[blockIndex] = saveData.BlockPalette[saveData.ChunkBlocks.Span[blockIndex]];
            }
            binaryWriter.Write(encodedBytes);

            if (world.Directory == null) return;
            
            FileStream fs = new FileStream(Path.Combine(world.Directory, GetFilename(chunkCoords, world, optimizeSave)), FileMode.Create);

            
            if (optimizeSave)
            {

                string uncompressedPath = Path.Combine(world.Directory, $"{world.Name}_x_{saveData.Location.X}-y_{saveData.Location.Y}.cdat");
                if (File.Exists(uncompressedPath))
                {
                    File.Delete(uncompressedPath);
                }

                byte[] cdata = Compress(outputData.GetBuffer());
                fs.Write(cdata);
            }
            else
            {
                fs.Write(outputData.GetBuffer());
            }
            
            binaryWriter.Close();
            outputData.Close(); 
            fs.Close();


        }

        public static byte[] Compress(ReadOnlySpan<byte> data)
        {
            using MemoryStream memoryStream = new MemoryStream();
            using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
            {
                deflateStream.Write(data);
            }
            return memoryStream.GetBuffer();
        }

    }
}