using System;
using Engine.MathLib;
using MCClone_Core.World_CS.Generation.Noise;
using Random = Engine.Random.Random;

namespace MCClone_Core.World_CS.Generation.Chunk_Generator_cs
{
    internal class BaseGenerator
    {
        internal const int MinTerrainHeight = 8;

        internal const int MaxTerrainCaveHeight = 20;
        
        internal int GenHeight = 60;


        public int[,] GenerateHeightmap(int x, int z, long seed)
        {
            NoiseUtil noise = new NoiseUtil(seed);
            noise.SetFractalOctaves(1);
            //noise.SetFrequency(0.0001f);
            MixedNoiseClass heightNoise = new MixedNoiseClass(3, noise);

            int[,] groundHeight = new int[(int)ChunkCs.Dimension.X, (int)ChunkCs.Dimension.Z];

            for (int xIndex = 0; xIndex < ChunkCs.Dimension.X; xIndex++)
            {
                for (int zIndex = 0; zIndex < ChunkCs.Dimension.Z; zIndex++)
                {
                    double hNoise = MathHelper.Clamp(((1f + heightNoise.GetMixedNoiseSimplex(x + xIndex, z + zIndex)))/2,  0, 1);
                    int yHeight = (int) (hNoise * (GenHeight - 1) + 1);
                    
                    groundHeight[xIndex,zIndex] = yHeight;
                }
            }

            return groundHeight;
            
        }

        public void Generate(ChunkCs  chunk, int x, int z, long seed)
        {
            x *= (int)ChunkCs.Dimension.X;
            z *= (int)ChunkCs.Dimension.Z;
            
            int[,] surfaceheight = GenerateHeightmap(x,z,seed);

            for (int xIndex = 0; xIndex < ChunkCs.Dimension.X; xIndex++)
            {
                for (int zIndex = 0; zIndex < ChunkCs.Dimension.Z; zIndex++)
                {
                    generate_surface(chunk ,surfaceheight[xIndex,zIndex], xIndex, zIndex); 
                    GenerateTopsoil(chunk,surfaceheight[xIndex,zIndex], xIndex, zIndex, seed);	
                }
            }
            
            Generate_Caves(chunk, seed, surfaceheight, x, z);
            generate_details(chunk,ProcWorld.WorldRandom,surfaceheight);
        }

        public virtual void generate_surface(ChunkCs chunk,int height, int x, int z)
        {
            chunk._set_block_data(x,0,z, 0);   
        }

        public virtual void GenerateTopsoil(ChunkCs chunk, int height, int x, int z, long seed)
        {
            
        }

        public virtual void generate_details(ChunkCs chunk, Random rng, int[,] groundHeight, bool checkingInterChunkGen = true)
        {
        }

        public virtual void Generate_Caves(ChunkCs chunk, long seed, int[,] height, int locX, int locZ)
        {
            NoiseUtil noisegen = new NoiseUtil();
            noisegen.SetSeed(seed);
			
            noisegen.SetFractalOctaves(100);

            for (int z = 0; z < ChunkCs.Dimension.Z; z++)
            {
                for (int x = 0; x < ChunkCs.Dimension.X; x++)
                {
                    for (int y = 0 + MinTerrainHeight; y == height[x, z]; y++)
                    {
                       double caveNoise = Math.Abs(noisegen.GetSimplex(x + locX, y, z + locZ));

                        if (caveNoise <= .3f)
                        {
                            chunk._set_block_data(x,y,z,0);
                        }
                    }
                }
            }
        }
        
        internal static bool IsBlockAir(ChunkCs chunk, int x, int y, int z)
        {
            return false;  //Chunk.BlockData[ChunkCs.GetFlattenedDimension(X, Y - 1, Z)] == 0;
        }


    }
}