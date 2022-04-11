#if !Core
using Godot;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Text;
using MCClone_Core.World_CS.Generation;

namespace MCClone_Core.Utility.IO
{
    public class ChunkFilesV2 : BaseFileHandler
    {
        public new const byte VersionNumber = 2;

        public override ChunkCs GetChunkData(WorldData world, Int2 location, out bool chunkExists)
        {
            // TODO Come up with newer and shorter filename structure that will work when I batch chunks together
            string filename = GetFilename(location, world,false);
            string filePath = Path.Combine(world.Directory, filename);
            
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(world.Directory, GetFilename(location, world, false));
                if (!File.Exists(filePath))
                {
                    chunkExists = false;
                    return null;
                }
            }
            
            byte[] FileData = File.ReadAllBytes(filePath);
            
            
            MemoryStream data = new MemoryStream(FileData);
            BinaryReader fileReader = new BinaryReader(data);


            SaveInfo saveData = new SaveInfo();
            
            saveData.VersionNumber = fileReader.ReadByte();
            
            saveData.BlockSize = fileReader.ReadByte();

            int x = fileReader.ReadInt32();
            int y = fileReader.ReadInt32();
            saveData.Location = new Int2(x, y);
            
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

            int BlockRuns = (int)((fileReader.BaseStream.Length - fileReader.BaseStream.Position) / 3);
            Run[] runs = new Run[BlockRuns];

            byte[] worldData = new byte[ChunkCs.MaxX * ChunkCs.MaxY * ChunkCs.MaxZ];
            
            var Runs = fileReader.ReadUInt16();
            for (int run = 0; run < Runs; run++)
            {
                runs[run].value = fileReader.ReadByte();
                runs[run].length = fileReader.ReadUInt16();
            }

            var worldIndex = 0;
            for (int i = 0; i < runs.Length; i++)
            {
                var run = runs[i];
                var blockid = blockDict[run.value];
                Console.WriteLine($"Writing {blockid} {run.length} Times");
                for (int length = 0 + worldIndex; length < run.length + worldIndex; length++)
                {
                    worldData[length] = blockid;
                }

                worldIndex += run.length;
            }

            saveData.ChunkBlocks = worldData;
            
            
            
            
            //byte[] serializedBlockData = fileReader.ReadBytes(ChunkCs.MaxX * ChunkCs.MaxY * ChunkCs.MaxZ);
            
            


            //saveData.ChunkBlocks = new byte[ChunkCs.MaxX * ChunkCs.MaxY * ChunkCs.MaxZ];
            
            
            
            /*for (int i = 0; i <  serializedBlockData.Length; i++)
            {
                saveData.ChunkBlocks[i] = blockDict[serializedBlockData[i]];
            }*/

            data.Close();

            ChunkCs referencedChunk = new ChunkCs
            {
                BlockData = saveData.ChunkBlocks,
                ChunkCoordinate = saveData.Location,
                Pos = new Vector3(ChunkCs.MaxX * saveData.Location.X, 0, ChunkCs.MaxX * saveData.Location.Y)

            };
            


            chunkExists = true;
            return referencedChunk;
        }

        struct Run
        {
            public ushort length; 
            public byte value;
        }


        List<Run> GetRuns(byte[] blocks, Dictionary<byte,byte> palette)
        {
            var runs = new List<Run>();

            byte lastPalleteValue = blocks[0];
            ushort Count = 1;
            Run CurrentRun = new Run()
            {
                value = palette[blocks[0]],
                length = 1
            };
            
            
            for (int blockIndex = 0; blockIndex < blocks.Length; blockIndex++)
            {
                byte currentBlock = blocks[blockIndex];
                if(currentBlock != lastPalleteValue|| Count == ushort.MaxValue)
                {
                    //Console.WriteLine($"Adding run with value: {CurrentRun.value}, length: {CurrentRun.length}");
                    runs.Add(CurrentRun);
                    lastPalleteValue = currentBlock;
                    CurrentRun = new Run()
                    {
                        value = palette[currentBlock],
                        length = Count
                    };
                    Count = 1;
                    continue;
                }
                Count++;
            }
            runs.Add(CurrentRun);

            return runs;
        }

        public override void WriteChunkData(byte[] blocks, Int2 chunkCoords, WorldData world, bool optimizeSave = false)
        {

            MemoryStream chunkdat = new MemoryStream();
            BinaryWriter fileWriter = new BinaryWriter(chunkdat, Encoding.Default, true);

            SaveInfo saveData = SerializeChunkData(blocks,chunkCoords, world, optimizeSave);
            fileWriter.Write(saveData.VersionNumber);
            fileWriter.Write(saveData.BlockSize);
            
            fileWriter.Write(saveData.Location.X);
            fileWriter.Write(saveData.Location.Y);
            
            fileWriter.Write(saveData.BiomeId);

            fileWriter.Write((short)saveData.BlockPalette.Count);
            foreach ((byte key, byte value) in saveData.BlockPalette)
            {
                fileWriter.Write(key);
                fileWriter.Write(value);
            }
            
            var Runs = GetRuns(blocks, saveData.BlockPalette);
            fileWriter.Write((ushort)Runs.Count);
            for (int i = 0; i < Runs.Count; i++)
            {
                fileWriter.Write(Runs[i].value);
                fileWriter.Write(Runs[i].length);
            }
            

            /*Span<byte> encodedBytes = stackalloc byte[saveData.ChunkBlocks.Length];
            for (int blockIndex = 0; blockIndex < encodedBytes.Length; blockIndex++)
            {
                encodedBytes[blockIndex] = saveData.BlockPalette[saveData.ChunkBlocks[blockIndex]];
            }*/
            //fileWriter.Write(encodedBytes);
            fileWriter.Close();

            if(world.Directory == null) return;
            
            
            
            FileStream fs = new FileStream(Path.Combine(world.Directory, GetFilename(chunkCoords, world, optimizeSave)), FileMode.Create);
            fs.Write(chunkdat.GetBuffer(), 0, (int)chunkdat.Length);
            chunkdat.Close();
            fs.Close();


        }

        public static Memory<byte> Compress(byte[] data)
        {

            Memory<byte> compressArray = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZLibStream libStream = new ZLibStream(memoryStream, CompressionLevel.Fastest, true))
                {
                    libStream.Write(data, 0, data.Length);
                }
                compressArray = memoryStream.GetBuffer().AsMemory(0, (int) memoryStream.Length);
            }
            return compressArray;
        }

    }
}