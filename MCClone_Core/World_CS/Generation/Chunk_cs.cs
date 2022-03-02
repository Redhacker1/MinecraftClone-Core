using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Objects;
using Engine.Renderable;
using MCClone_Core.Temp;
using MCClone_Core.World_CS.Blocks;
using MCClone_Core.World_CS.Generation.Chunk_Generator_cs;

namespace MCClone_Core.World_CS.Generation
{

	public class ChunkCs : GameObject
	{
		List<BlockStruct> palette = new List<BlockStruct>();
		
		//bool NeedsSaved;

		public Vector2 ChunkCoordinate;

		public readonly Dictionary<Vector2, ChunkCs> NeighbourChunks = new Dictionary<Vector2, ChunkCs>();


		static readonly Vector2 TextureAtlasSize = new Vector2(8, 4);
			
		static readonly float Sizex = 1.0f / TextureAtlasSize.X;
		static readonly float Sizey = 1.0f / TextureAtlasSize.Y;

		public const int MaxX = 16;
		public const int MaxY = 384;
		public const int MaxZ = 16;
		//public static readonly Vector3 Max = new Vector3(16, 384, 16);
		Mesh ChunkMesh;


		static readonly Vector3[] V =
		{
			new Vector3(0, 0, 0), //0
			new Vector3(1, 0, 0), //1
			new Vector3(0, 1, 0), //2
			new Vector3(1, 1, 0), //3
			new Vector3(0, 0, 1), //4
			new Vector3(1, 0, 1), //5
			new Vector3(0, 1, 1), //6
			new Vector3(1, 1, 1)  //7
		};

		static readonly int[] Top = {2, 3, 7, 6};
		static readonly int[] Bottom = {0, 4, 5, 1};
		static readonly int[] Left = {6, 4, 0, 2};
		static readonly int[] Right = {3, 1, 5, 7};
		static readonly int[] Front = {7, 5, 4, 6};
		static readonly int[] Back = {2, 0, 1, 3};
		static readonly int[] Cross1 = {3, 1, 4, 6};
		static readonly int[] Cross2 = {7, 5, 0, 2};
		static readonly int[] Cross3 = {6, 4, 1, 3};
		static readonly int[] Cross4 = {2, 0, 5, 7};
		
		static readonly BaseGenerator Generator = new ForestGenerator();

		public byte[] BlockData;
		readonly bool[,,] _visibilityMask;
		public bool ChunkDirty;

		public ChunkCs()
		{

			BlockData = new byte[MaxX * MaxY * MaxZ];
			_visibilityMask = new bool[MaxX, MaxY, MaxZ];
			ChunkMesh = new Mesh(this, ProcWorld.Instance._material);
			ProcWorld.Instance._material.AddReference(ChunkMesh);
		}


		public void InstantiateChunk(ProcWorld w, int cx, int cz, long seed)
		{
			
			Pos = new Vector3(cx * (MaxX), 0, cz * MaxZ);
			ChunkCoordinate = new Vector2(cx, cz);
			
			Generator.Generate(this, cx, cz, seed);
		}

		static HeapPool pool = new HeapPool(NativeMemoryHeap.Instance, uint.MaxValue);
        

		public void Update()
		{
            ByteStore<Vector3> BlockVerts = ByteStore<Vector3>.Create(pool, 1);
            ByteStore<Vector3> blocksNormals = ByteStore<Vector3>.Create(pool, 1);
            ByteStore<uint> chunkIndices = ByteStore<uint>.Create(pool, 1);
            ByteStore<Vector3> BlockUVs = ByteStore<Vector3>.Create(pool, 1);
			Span<bool> transparent = stackalloc bool[6];
			uint index = 0;
			
			//Making use of multidimensional arrays allocated on creation
			for (int x = 0; x < MaxX; x++)
			for (int y = 0; y < MaxY; y++)
			for (int z = 0; z < MaxZ; z++)
			{
				byte block = BlockData[GetFlattenedMax(x, y, z)];
				if (block == 0) continue;
				check_transparent_neighbours(x, y, z, ref transparent);
				//TODO: AO Code goes here!
				if (transparent.Contains(true))
				{
					_create_block(transparent, x, y, z, block, BlockVerts, blocksNormals, BlockUVs, chunkIndices,
						ref index);
				}
			}

			ChunkMesh.GenerateMesh(BlockVerts.Span, BlockUVs.Span, chunkIndices.Span);
            BlockVerts.Dispose();
            BlockUVs.Dispose();
            blocksNormals.Dispose();
            chunkIndices.Dispose();
            BlockUVs.Dispose();
		}


		public void UpdateVisMask()
		{
			for (int z = 0; z < MaxZ; z++)
			for (int y = 0; y < MaxY; y++)
			for (int x = 0; x < MaxX; x++)
			{
				_visibilityMask[x,y,z] = BlockHelper.BlockTypes[BlockData[GetFlattenedMax(x,y,z)]].Transparent;
			}
		}
	

		public void _set_block_data(int x, int y, int z, byte b, bool overwrite = true)
		{
			if (x >= 0 && x < MaxX && y >= 0 && y < MaxY && z >= 0 && z < MaxZ)
			{
				if (!overwrite && BlockData[GetFlattenedMax(x, y, z)] != 0) return;
				BlockData[GetFlattenedMax(x, y, z)] = b;

				_visibilityMask[x,y,z] = BlockHelper.BlockTypes[b].Transparent;
				ChunkDirty = true;
				//NeedsSaved = true;
			}
			else
			{
				//GD.Print("External Chunk Write");

				Vector3 worldCoordinates = new Vector3(x + Pos.X, y, z + Pos.Z);
				int localX = (int) (MathHelper.Modulo(Math.Floor(worldCoordinates.X), MaxX) + 0.5);
				int localY = (int) (MathHelper.Modulo(Math.Floor(worldCoordinates.Y), MaxY) + 0.5);
				int localZ = (int) (MathHelper.Modulo(Math.Floor(worldCoordinates.Z), MaxZ) + 0.5);

				int cx = (int) Math.Floor(worldCoordinates.X / MaxX);
				int cz = (int) Math.Floor(worldCoordinates.Z / MaxZ);
				
				Vector2 chunkKey = new Vector2(cx, cz);
				if (NeighbourChunks.ContainsKey(chunkKey))
				{
					NeighbourChunks[chunkKey]._set_block_data(localX, localY, localZ, b, overwrite);
				}
				else if(ProcWorld.Instance.LoadedChunks.ContainsKey(chunkKey))
				{
					ChunkCs currentChunk = ProcWorld.Instance.LoadedChunks[chunkKey];
					NeighbourChunks[chunkKey] = currentChunk;
					currentChunk?._set_block_data(localX,localY,localZ,b,overwrite);
				}
			}
		}

		void check_transparent_neighbours(int x, int y, int z, ref Span<bool> output, bool DiscardOnlyAir = false)
		{
			output[0] = is_block_transparent(x, y + 1, z, DiscardOnlyAir);
			output[1] = is_block_transparent(x, y - 1, z, DiscardOnlyAir);
			output[2] = is_block_transparent(x - 1, y, z, DiscardOnlyAir);
			output[3] = is_block_transparent(x + 1, y, z, DiscardOnlyAir);
			output[4] = is_block_transparent(x, y, z - 1, DiscardOnlyAir);
			output[5] = is_block_transparent(x, y, z + 1, DiscardOnlyAir);
		}

		void _create_block(Span<bool> check, int x, int y, int z, byte block, ByteStore<Vector3> blocks, ByteStore<Vector3> blocksNormals, ByteStore<Vector3> uVs, ByteStore<uint> indices, ref uint index)
		{
			List<BlockStruct> blockTypes = BlockHelper.BlockTypes;
			Vector3 coord = new Vector3(x, y, z);
			if (blockTypes[block].TagsList.Contains("Flat"))
			{
				Vector2 tempvar = blockTypes[block].Only;
				create_face(Cross1, ref coord, tempvar, blocks, blocksNormals, uVs, indices, ref index);
				create_face(Cross2, ref coord, tempvar, blocks, blocksNormals, uVs, indices, ref index);
				create_face(Cross3, ref coord, tempvar, blocks, blocksNormals, uVs, indices, ref index);
				create_face(Cross4, ref coord, tempvar, blocks, blocksNormals, uVs, indices, ref index);
			}
			else
			{
				if (check[0]) create_face(Top, ref coord, blockTypes[block].Top, blocks, blocksNormals, uVs, indices, ref index);
				if (check[1]) create_face(Bottom, ref coord, blockTypes[block].Bottom, blocks, blocksNormals, uVs, indices, ref index);
				if (check[2]) create_face(Left, ref coord, blockTypes[block].Left, blocks, blocksNormals, uVs, indices, ref index);
				if (check[3]) create_face(Right, ref coord, blockTypes[block].Right, blocks, blocksNormals, uVs, indices, ref index);
				if (check[4]) create_face(Back, ref coord, blockTypes[block].Back, blocks, blocksNormals, uVs, indices,ref index);
				if (check[5]) create_face(Front, ref coord, blockTypes[block].Front, blocks, blocksNormals, uVs, indices, ref index);
			}
		}

		void create_face(IReadOnlyList<int> I, ref Vector3 offset, Vector2 textureAtlasOffset, ByteStore<Vector3> blocks, ByteStore<Vector3> blocksNormals, ByteStore<Vector3> uVs, ByteStore<uint> indices, ref uint currentindex)
		{
			blocks.PrepareCapacityFor(4);
			indices.PrepareCapacityFor(6);
			uVs.PrepareCapacityFor(4);


			Vector3 uvOffset = new Vector3(
				textureAtlasOffset.X / TextureAtlasSize.X,
				textureAtlasOffset.Y / TextureAtlasSize.Y, 0
			);

			// the f means float, there is another type called double it defaults to that has better accuracy at the cost of being larger to store, but vector3 does not use it.
			Vector3 uvB = new Vector3(uvOffset.X, Sizey + uvOffset.Y, 0);
			Vector3 uvC = new Vector3(Sizex, Sizey, 0) + uvOffset;
			Vector3 uvD = new Vector3(Sizex + uvOffset.X, uvOffset.Y, 0);
			
			Span<uint> Indices = stackalloc uint[6] {currentindex, currentindex + 1, currentindex + 2, currentindex, currentindex + 2, currentindex + 3};
			Span<Vector3> Verts = stackalloc Vector3[4]
				{V[I[0]] + offset, V[I[1]] + offset, V[I[2]] + offset, V[I[3]] + offset};

			blocks.AppendRange(Verts);
			Verts[0] = uvOffset;
			Verts[1] = uvB;
			Verts[2] = uvC;
			Verts[3] = uvD;
			uVs.AppendRange(Verts);	
			indices.AppendRange(Indices);
			currentindex += 4;

			//blocksNormals.AddRange(NormalGenerate(a, b, c, d));
		}
		
		static IEnumerable<Vector3> NormalGenerate(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			// HACK: Actually calculate normals as this only works for cubes

			Vector3 qr = c - a;
			Vector3 qs = b - a;

			Vector3 normal = new Vector3((qr.Y * qs.Z) - (qr.Z * qs.Y),(qr.Z * qs.X) - (qr.X * qs.Z), (qr.X * qs.Y) - (qr.Y * qs.X) );

			return new[] {normal, normal, normal, normal, normal, normal};

		}

		public static int GetFlattenedMax(int x, int y, int z)
		{
			return x + y * MaxZ + z * MaxY * MaxX;
		}
		
		bool is_block_transparent(int x, int y, int z, bool DiscardAir = false)
		{
			if (x < 0 || x >= MaxX || z < 0 || z >= MaxZ)
			{
				
				int cx = x / MaxX;
				int cz = z / MaxX;

				int bx = (int) (MathHelper.Modulo(MathF.Floor(x), MaxX));
				int bz = (int) (MathHelper.Modulo(MathF.Floor(z), MaxZ));

				Vector2 cpos = new Vector2(cx, cz);


				if (ProcWorld.Instance.LoadedChunks.ContainsKey(cpos))
				{
					if (DiscardAir)
					{
						int Index = GetFlattenedMax(bx, y, bz);
						return BlockHelper.BlockTypes[ProcWorld.Instance.LoadedChunks[cpos].BlockData[Index]].Air;
					}
					
					return ProcWorld.Instance.LoadedChunks[cpos]._visibilityMask[bx, y, bz];
				}
				return false;
			}

			if (y < 0 || y >= MaxY)
			{
				return false;	
			}
			
			
			if (DiscardAir)
			{
				return BlockHelper.BlockTypes[BlockData[GetFlattenedMax(x,y,z)]].Air;
			}
			return _visibilityMask[x,y,z];
		}
		
		protected override void OnFree()
		{
			base.OnFree();
			ChunkMesh.Dispose();
			ProcWorld.Instance._material.RemoveReference(ChunkMesh);
            //pool.
		}
		

		byte vertexAO(byte side1, byte side2, byte corner) {
			if(side1 == side2)
			{
				return 0;
			}

			return (byte)(3 - (side1 + side2 + corner));
		}
	}
}
	
