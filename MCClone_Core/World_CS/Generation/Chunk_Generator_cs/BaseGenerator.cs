using System;
using Engine.MathLib;
using Engine.MathLib.Random;
using MCClone_Core.World_CS.Blocks;
using Random = Engine.MathLib.Random.Random;

namespace MCClone_Core.World_CS.Generation.Chunk_Generator_cs
{
    internal class BaseGenerator
    {
        internal const int MinTerrainHeight = 8;

        internal const int MaxTerrainCaveHeight = 20;
        
        internal int GenHeight = 60;


        public int[,] GenerateHeightmap(int X, int Z, long seed)
        {
            NoiseUtil noise = new NoiseUtil(seed);
            noise.SetFractalOctaves(1);
            //noise.SetFrequency(0.0001f);
            MixedNoiseClass HeightNoise = new MixedNoiseClass(3, noise);

            int[,] groundHeight = new int[ChunkCs.MaxX, ChunkCs.MaxZ];

            for (int x = 0; x < ChunkCs.MaxX; x++)
            {
                for (int z = 0; z < ChunkCs.MaxZ; z++)
                {
                    double hNoise = MathHelper.Clamp(((1f + HeightNoise.GetMixedNoiseSimplex(x + X, z + Z)))/2,  0, 1);
                    int yHeight = (int) (hNoise * (GenHeight - 1) + 1);
                    
                    groundHeight[x,z] = yHeight;
                }
            }

            return groundHeight;
            
        }

        public void Generate(ChunkCs  chunk, int X, int Z, long Seed)
        {
            X *= ChunkCs.MaxX;
            Z *= ChunkCs.MaxZ;
            
            int[,] surfaceheight = GenerateHeightmap(X,Z,Seed);

            for (int x = 0; x < ChunkCs.MaxX; x++)
            {
                for (int z = 0; z < ChunkCs.MaxZ; z++)
                {
                    generate_surface(chunk ,surfaceheight[x,z], x, z); 
                    GenerateTopsoil(chunk,surfaceheight[x,z], x, z, Seed);	
                }
            }
            
            Generate_Caves(chunk, Seed, surfaceheight, X, Z);
            generate_details(chunk,ProcWorld.WorldRandom,surfaceheight);
        }

        public virtual void generate_surface(ChunkCs Chunk,int Height, int X, int Z)
        {
            Chunk._set_block_data(X,0,Z, 0);   
        }

        public virtual void GenerateTopsoil(ChunkCs Chunk, int Height, int X, int Z, long seed)
        {
            
        }

        public virtual void generate_details(ChunkCs Chunk, Random Rng, int[,] GroundHeight, bool CheckingInterChunkGen = true)
        {
        }

        public virtual void Generate_Caves(ChunkCs Chunk, long Seed, int[,] Height, int LocX, int LocZ)
        {
            NoiseUtil Noisegen = new NoiseUtil();
            Noisegen.SetSeed(Seed);
			
            Noisegen.SetFractalOctaves(100);

            for (int Z = 0; Z < ChunkCs.MaxZ; Z++)
            {
                for (int X = 0; X < ChunkCs.MaxX; X++)
                {
                    for (int Y = 0 + MinTerrainHeight; Y == Height[X, Z]; Y++)
                    {
                       double CaveNoise = Math.Abs(Noisegen.GetSimplex(X + LocX, Y, Z + LocZ));

                        if (CaveNoise <= .3f)
                        {
                            Chunk._set_block_data(X,Y,Z,0);
                        }
                    }
                }
            }
        }
        
        internal static bool IsBlockAir(ChunkCs Chunk, int X, int Y, int Z)
        {
            return BlockHelper.BlockTypes[Chunk.BlockData[ChunkCs.GetFlattenedIndex(X, Y - 1, Z)]].Air;
        }


    }
}