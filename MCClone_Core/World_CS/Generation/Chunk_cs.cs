using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.MathLib;
using Engine.Objects;
using Engine.Objects.SceneSystem;
using Engine.Utilities.MathLib;
using MCClone_Core.Temp;
using MCClone_Core.World_CS.Blocks;

namespace MCClone_Core.World_CS.Generation
{

	public class ChunkCs : EngineObject
	{
		internal static ConcurrentDictionary<Vector2, ChunkCs> LoadedChunks;
		readonly List<BlockStruct> _blockTypes = BlockHelper.BlockTypes;
		Dictionary<byte, BlockStruct> _palette = new Dictionary<byte, BlockStruct>();
		
		//bool NeedsSaved;

		public Int2 ChunkCoordinate;

		public readonly Dictionary<Int2, ChunkCs> NeighbourChunks = new Dictionary<Int2, ChunkCs>();


		static readonly Int2 TextureAtlasSize = new Int2(8, 4);
			
		static readonly float SizeX = 1.0f / TextureAtlasSize.X;
		static readonly float SizeY = 1.0f / TextureAtlasSize.Y;

		public const int MaxX = 16;
		public const int MaxY = 384;
		public const int MaxZ = 16;
		readonly ChunkMesh _chunkMesh;


		static readonly Int3[] V =
		{
			new Int3(0, 0, 0), //0
			new Int3(1, 0, 0), //1
			new Int3(0, 1, 0), //2
			new Int3(1, 1, 0), //3
			new Int3(0, 0, 1), //4
			new Int3(1, 0, 1), //5
			new Int3(0, 1, 1), //6
			new Int3(1, 1, 1)  //7
		};

		static readonly uint[] Top = {2, 3, 7, 6};
		static readonly uint[] Bottom = {0, 4, 5, 1};
		static readonly uint[] Left = {6, 4, 0, 2};
		static readonly uint[] Right = {3, 1, 5, 7};
		static readonly uint[] Front = {7, 5, 4, 6};
		static readonly uint[] Back = {2, 0, 1, 3};
		static readonly uint[] Cross1 = {3, 1, 4, 6};
		static readonly uint[] Cross2 = {7, 5, 0, 2};
		static readonly uint[] Cross3 = {6, 4, 1, 3};
		static readonly uint[] Cross4 = {2, 0, 5, 7};

		internal readonly Instance3D Instance3D;

		public readonly SafeByteStore<byte> BlockData;
		readonly SafeByteStore<bool> _visibilityMask;
		public bool ChunkDirty;

		public ChunkCs()
		{
			BlockData = SafeByteStore<byte>.Create(ChunkSingletons.ChunkPool, MaxX * MaxY * MaxZ);
			BlockData.EnsureCapacity(MaxX * MaxY * MaxZ);
			BlockData.GetAppendSpan(MaxX * MaxY * MaxZ);
			
			_visibilityMask = SafeByteStore<bool>.Create(ChunkSingletons.ChunkPool, MaxX * MaxY * MaxZ);
			_visibilityMask.EnsureCapacity(MaxX * MaxY * MaxZ);
			_visibilityMask.GetAppendSpan(MaxX * MaxY * MaxZ);

			if (BlockData.Capacity == 0)
			{
				throw new InvalidOperationException();
			}

			_chunkMesh = new ChunkMesh();
			
			Instance3D = new Instance3D(_chunkMesh, ProcWorld.Instance._material);
			AddChild(Instance3D);
			
			MinecraftCloneCore.Scene.AddInstance(Instance3D);
			
		}


		public void InstantiateChunk(int cx, int cz, long seed)
		{

			for (int i = 0; i < BlockData.Span.Length; i++)
			{
				BlockData.Span[i] = 0;
			}
			
			for (int i = 0; i < BlockData.Span.Length; i++)
			{
				BlockData.Span[i] = 0;
			}
			
			
			
			Position = new Vector3(cx * MaxX, 0, cz * MaxZ);
			ChunkCoordinate = new Int2(cx, cz);
			
			ChunkSingletons.Generator.Generate(this, cx, cz, seed);
		}


		readonly SafeByteStore<int> _blockVerts = SafeByteStore<int>.Create(ChunkSingletons.ChunkPool, 1);
		readonly SafeByteStore<Vector3> _blocksNormals = SafeByteStore<Vector3>.Create(ChunkSingletons.ChunkPool, 1);
		readonly SafeByteStore<Vector2> _blockUVs = SafeByteStore<Vector2>.Create(ChunkSingletons.ChunkPool, 1);

		readonly Stopwatch _stopwatch =Stopwatch.StartNew();
		
		readonly object _renderLock = new object();
		public void Update()
		{

			if (_freed)
			{
				return;
			}
			
			_stopwatch.Restart();
			
			_blockVerts.Clear();
			_blocksNormals.Clear();
			_blockUVs.Clear();
			
			
			Span<bool> transparent = stackalloc bool[6];

			ReadOnlySpan<byte> BlockSpan = BlockData.Span;

			uint faces = 0;


			for (int z = 0; z < MaxZ; z++)
			for (int y = 0; y < MaxY; y++)
			for (int x = 0; x < MaxX; x++)
			{
	
				byte block = BlockSpan[GetFlattenedIndex(x, y, z)];
				if (block == 0 || _blockTypes[block].Air)
				{
					continue;
				}
				check_transparent_neighbours(x, y, z, transparent);

				//TODO: AO Code goes here!
				if (transparent.Contains(true))
				{
					foreach (bool faceVisible in transparent)
					{
						if (faceVisible)
						{
							faces++;
						}
					}
					
					_create_block(transparent, x, y, z, block, _blockVerts, _blocksNormals, _blockUVs);
				}
			}
			
			lock (_renderLock)
			{
				_chunkMesh.GenerateMesh(_blockVerts.Span, _blockUVs.Span, faces * 6);
			}
			Instance3D.SetTransform(this);
			
		}
		
		


		public void UpdateVisMask()
		{
			for (int z = 0; z < MaxZ; z++)
			for (int y = 0; y < MaxY; y++)
			for (int x = 0; x < MaxX; x++)
			{
				int index = GetFlattenedIndex(x, y, z);
				_visibilityMask.Span[index] = BlockHelper.BlockTypes[BlockData.Span[index]].Transparent;
			}
		}
	

		public void _set_block_data(int x, int y, int z, byte b, bool overwrite = true)
		{
			if (x >= 0 && x < MaxX && y >= 0 && y < MaxY && z >= 0 && z < MaxZ)
			{
				var index = GetFlattenedIndex(x, y, z);
				if (!overwrite && BlockData.Span[index] != 0) return;
				BlockData.Span[index] = b;

				_visibilityMask.Span[index] = BlockHelper.BlockTypes[b].Transparent;
				ChunkDirty = true;
				//NeedsSaved = true;
			}
			else
			{
				//GD.Print("External Chunk Write");

				Vector3 worldCoordinates = new Vector3(x + Position.X, y, z + Position.Z);
				
				int cx = ChunkCoordinate.X;
				int cz = ChunkCoordinate.Y;
				int localX = MathHelper.Modulo(cx, MaxX);
				int localY = (int) (MathHelper.Modulo(Math.Floor(worldCoordinates.Y), MaxY) + 0.5);
				int localZ = MathHelper.Modulo(cz, MaxZ);
				

				Int2 chunkpos = new Int2(cx, cz);
				Vector2 chunkKey = new Vector2(cx, cz);
				if (NeighbourChunks.ContainsKey(chunkpos))
				{
					NeighbourChunks[chunkpos]._set_block_data(localX, localY, localZ, b, overwrite);
				}
				else if (LoadedChunks.ContainsKey(chunkKey))
				{
					ChunkCs currentChunk = LoadedChunks[chunkKey];
					NeighbourChunks[chunkpos] = currentChunk;
					currentChunk?._set_block_data(localX, localY, localZ, b, overwrite);
				}
			}
		}

		void check_transparent_neighbours(int x, int y, int z, Span<bool> output, bool discardOnlyAir = false)
		{
			output[0] = is_block_transparent(x, y + 1, z, discardOnlyAir);
			output[1] = is_block_transparent(x, y - 1, z, discardOnlyAir);
			output[2] = is_block_transparent(x - 1, y, z, discardOnlyAir);
			output[3] = is_block_transparent(x + 1, y, z, discardOnlyAir);
			output[4] = is_block_transparent(x, y, z - 1, discardOnlyAir);
			output[5] = is_block_transparent(x, y, z + 1, discardOnlyAir);
		}

		void _create_block(Span<bool> check, int x, int y, int z, byte block, SafeByteStore<int> blocks, SafeByteStore<Vector3> blocksNormals, SafeByteStore<Vector2> uVs)
		{
			List<BlockStruct> blockTypes = BlockHelper.BlockTypes;
			Int3 coord = new Int3(x, y, z);
			if (blockTypes[block].TagsList.Contains("Flat"))
			{
				Vector2 Only = blockTypes[block].Only;
				create_face(Cross1, ref coord, Only, blocks, blocksNormals, uVs);
				create_face(Cross2, ref coord, Only, blocks, blocksNormals, uVs);
				create_face(Cross3, ref coord, Only, blocks, blocksNormals, uVs);
				create_face(Cross4, ref coord, Only, blocks, blocksNormals, uVs);
			}
			else
			{
				if (check[0]) create_face(Top, ref coord, blockTypes[block].Top, blocks, blocksNormals, uVs);
				if (check[1]) create_face(Bottom, ref coord, blockTypes[block].Bottom, blocks, blocksNormals, uVs);
				if (check[2]) create_face(Left, ref coord, blockTypes[block].Left, blocks, blocksNormals, uVs);
				if (check[3]) create_face(Right, ref coord, blockTypes[block].Right, blocks, blocksNormals, uVs);
				if (check[4]) create_face(Back, ref coord, blockTypes[block].Back, blocks, blocksNormals, uVs);
				if (check[5]) create_face(Front, ref coord, blockTypes[block].Front, blocks, blocksNormals, uVs);
			}
		}

        void create_face(uint[] I, ref Int3 offset, Vector2 textureAtlasOffset, SafeByteStore<int> blocks, SafeByteStore<Vector3> blocksNormals, SafeByteStore<Vector2> uVs)
        {
	        if (_freed)
	        {
		        return;
	        }
	        
	        blocks.PrepareCapacityFor(4);
	        uVs.PrepareCapacityFor(4);


	        Vector2 uvOffset = new Vector2(
		        textureAtlasOffset.X / TextureAtlasSize.X,
		        textureAtlasOffset.Y / TextureAtlasSize.Y
	        );

	        // the f means float, there is another type called double it defaults to that has better accuracy at the cost of being larger to store, but vector3 does not use it.
	        Vector2 uvB = new Vector2(uvOffset.X, SizeY + uvOffset.Y);
	        Vector2 uvC = new Vector2(SizeX, SizeY) + uvOffset;
	        Vector2 uvD = new Vector2(SizeX + uvOffset.X, uvOffset.Y);
	        
	        
	        Span<Vector2> uvs = stackalloc Vector2[4] { uvOffset, uvB, uvC, uvD};

	        for (int i = 0; i < 4; i++)
	        {
		        // Add Vertex
		        Int3 vert = V[I[i]] + offset;
		        int compressedPos = (vert.X << 5) | vert.Z;
		        compressedPos = compressedPos << 9 | vert.Y;
		        blocks.Append(compressedPos);
	        }


	        uVs.AppendRange(uvs);
        }
		
		static IEnumerable<Vector3> NormalGenerate(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			// HACK: Actually calculate normals as this only works for cubes

			Vector3 qr = c - a;
			Vector3 qs = b - a;

			Vector3 normal = new Vector3((qr.Y * qs.Z) - (qr.Z * qs.Y),(qr.Z * qs.X) - (qr.X * qs.Z), (qr.X * qs.Y) - (qr.Y * qs.X) );

			return new[] {normal, normal, normal, normal, normal, normal};

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetFlattenedIndex(int x, int y, int z)
		{
			return x + y * MaxZ + z * MaxY * MaxX;
		}
		
		
		bool is_block_transparent(int x, int y, int z, bool discardAir = false)
		{
			int index = GetFlattenedIndex(x, y, z);
			if (y < 0 || y >= MaxY)
			{
				return false;
			}

			if (x < 0 || x >= MaxX || z < 0 || z >= MaxZ)
			{
				int cx = (x / MaxX);
				int cz = (z / MaxZ);
				Vector2 cpos = new Vector2(cx, cz);

				if (ProcWorld.Instance.LoadedChunks.ContainsKey(cpos))
				{
					int bx = MathHelper.Modulo(x, MaxX);
					int bz = MathHelper.Modulo(z, MaxZ);

					return LoadedChunks[cpos].is_block_transparent(bx, y, bz, discardAir);
				}

				return false;
			}

			return discardAir ? BlockHelper.BlockTypes[BlockData.Span[index]].Air : _visibilityMask.Span[index];
		}
		
		protected override void OnFree()
		{
			base.OnFree();
			_blockVerts.Dispose();
			_blockUVs.Dispose();
			_blocksNormals.Dispose();
			_blockUVs.Dispose();
			_chunkMesh.Dispose();
			
			BlockData.Dispose();
			_visibilityMask.Dispose();
			MinecraftCloneCore.Scene.RemoveInstance(Instance3D);

			_freed = true;
		}

		bool _freed = false;


		byte VertexAo(byte side1, byte side2, byte corner) {
			if(side1 == side2)
			{
				return 0;
			}

			return (byte)(3 - (side1 + side2 + corner));
		}
	}
}
	
