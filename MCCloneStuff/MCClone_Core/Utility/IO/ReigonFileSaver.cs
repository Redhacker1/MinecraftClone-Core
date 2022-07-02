using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MCClone_Core.World_CS.Generation;

namespace MCClone_Core.Utility.IO
{
    class ReigonFile
    {
        public ReaderWriterLockSlim LockSlim = new ReaderWriterLockSlim();
        FileStream BackingFile;

        ReigonFile(string path, bool CreateFile)
        {
            if (File.Exists(path) == false && CreateFile)
            {
                BackingFile = File.Create(path, 1024, FileOptions.Asynchronous | FileOptions.RandomAccess);
            }
            else if(File.Exists(path) == false && CreateFile == false)
            {
                throw new FileNotFoundException("Could not find file!, Check spelling or use the CreateFile");
            }
            else
            {
                var FileOptions = new FileStreamOptions()
                {
                    Options = System.IO.FileOptions.Asynchronous | System.IO.FileOptions.RandomAccess,
                    Mode = FileMode.Open
                };
                BackingFile = File.Open(path, FileOptions);
            }
        }
        
        
        

    }
    
    public class ReigonFileHandler : BaseFileHandler
    {

        List<ReigonFile> reigons = new List<ReigonFile>();

        ConcurrentDictionary<Int2, ReigonFile> ReigonFilemap =
            new ConcurrentDictionary<Int2, ReigonFile>();

        const int RegionSize = 32;
        new const string FileExtension = "rgn";

        static int[] GetRegionFile(int x, int y)
        {
            int regionX = x >> 5;
            int regionZ = y >> 5;
            return new[]{regionX, regionZ};
        }
        static int[] GetChunkOffset(int x, int y)
        {
            int rx = x % RegionSize;
            int rz = y % RegionSize;
            return new[]{rx, rz};
        }

        public override string GetFilename(Int2 chunkCoords, WorldData world, bool compressed)
        {
            int[] xy = GetRegionFile((int)chunkCoords.X, (int)chunkCoords.Y);
            return $"{xy[0]}{xy[1]}.{FileExtension}";
        }

        public override bool ChunkExists(WorldData world, Int2 location)
        {
            if (File.Exists(GetFilename(location, world, false)))
            {
                return true;
            }
            return File.Exists(GetFilename(location, world, true));
        }

        public override ChunkCs GetChunkData(WorldData world, Int2 location, out bool chunkExists)
        {
            throw new NotImplementedException();
        }


        SaveInfo[] GetAllChunksFromFile(string filePath, long offset ,bool compressData)
        {
            throw new NotImplementedException();
        }

        public override void WriteChunkData(byte[] blocks, Int2 chunkCoords, WorldData world,
            bool optimizeSave = true)
        {
            throw new NotImplementedException();
        }
    }
}