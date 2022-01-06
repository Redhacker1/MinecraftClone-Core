using System;
using Engine.MathLib;
using MCClone_Core.World_CS.Generation.Noise;
using Random = Engine.Random.Random;

namespace MCClone_Core.World_CS.Generation.Chunk_Generator_cs
{
	internal class ForestGenerator : BaseGenerator
	{
		public override void generate_surface(ChunkCs chunk, int height, int x, int z)
		{
			for (int y = 0; y < height; y++)
			{
				//GD.Print($"{X},{Y},{Z}");
				chunk._set_block_data(x,y,z, 10);
			}
		}

		public override void GenerateTopsoil(ChunkCs chunk, int height, int x, int z, long seed)
		{	NoiseUtil heightNoise = new NoiseUtil();
			heightNoise.SetSeed(seed);
			heightNoise.SetFractalOctaves(100);


			double noise = heightNoise.GetSimplex(x + chunk.ChunkCoordinate.X, seed, z + chunk.ChunkCoordinate.Y);
			noise /= 2;

            int depth = (int) MathHelper.Lerp(1,6,Math.Abs(noise));

            for (int I = 0; I < depth; I++)
			{
				if (I == 0)
				{
					chunk._set_block_data(x,height - 1, z, 4);	
				}
				else
				{
					chunk._set_block_data(x,(height - 1) - I, z,4);	
				}
			}
		}

		public override void generate_details(ChunkCs chunk, Random rng, int[,] groundHeight, bool checkingInterChunkGen = true)
		{

			const int treeWidth = 2;

			for (int nTree = 0; nTree < rng.NextInt(2, 8); nTree++)
			{
				int posX = rng.NextInt(treeWidth, (int) ChunkCs.Dimension.X - treeWidth - 1);
				int posZ = rng.NextInt(treeWidth, (int) ChunkCs.Dimension.Z - treeWidth - 1);
				int treeHeight = rng.NextInt(4, 8);
				
				for (int I = 0; I < treeHeight; I++)
				{

					int x = posX;
					int z = posZ;
					
					int y = groundHeight[x,z] + I;
					
					// 6 is BID for logs
					chunk._set_block_data(x, y, z, 6);
				}
				int minY = rng.NextInt(-2, -1);

				int maxY = rng.NextInt(2, 4);

				for (int dy = minY; dy < maxY; dy++)
				{
					int leafWidth = treeWidth;
					if (dy == minY || dy == maxY - 1) leafWidth -= 1;
					for (int dx = -leafWidth; dx < leafWidth + 1; dx++)
					{
						for (int dz = -leafWidth; dz < leafWidth + 1; dz++)
						{
							int lx = posX + dx;
							int ly = groundHeight[posX,posZ] + treeHeight + dy;
							int lz = posZ + dz;
							
							
							// 5 is block ID for leaves
							chunk._set_block_data(lx, ly, lz, 5, false);
						}

						if (dy == minY || dy == maxY - 1) leafWidth -= 1;
					}
				}

				for (int nShrub = 0; nShrub < rng.NextInt(6, 10); nShrub++)
				{
					int x = rng.NextInt(0, (int)ChunkCs.Dimension.X - 1);
					int z = rng.NextInt(0, (int)ChunkCs.Dimension.Z - 1);
					int y = groundHeight[x,z];
					
					// 11 is block ID for tall grass
					if (!IsBlockAir(chunk, x, y, z))
					{
						chunk._set_block_data(x, y, z, 11, false);	
					}
				}

				for (int nFlower = 0; nFlower < rng.NextInt(4, 6); nFlower++)
				{
					int x = rng.NextInt(0, (int)ChunkCs.Dimension.X - 1);
					int z = rng.NextInt(0, (int)ChunkCs.Dimension.Z - 1);
					int y = groundHeight[x,z];
					
					// 3 is BID for flower
					if (!IsBlockAir(chunk, x, y, z))
					{
						chunk._set_block_data(x, y, z, 3, false);
					}
				}
			}
		}
	}
}
