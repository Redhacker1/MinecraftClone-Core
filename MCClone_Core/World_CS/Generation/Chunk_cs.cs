#if Core
#else
using Array = Godot.Collections.Array;
#endif

#if(Core)
#else
	using Godot;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Engine.MathLib;
using Engine.Objects;
using Engine.Renderable;
using MCClone_Core.Debug_and_Logging;
using MCClone_Core.World_CS.Blocks;
using MCClone_Core.World_CS.Generation.Chunk_Generator_cs;
using Vector2 = System.Numerics.Vector2;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace MCClone_Core.World_CS.Generation
{

	public class ChunkCs : GameObject
	{
		Mesh _chunkreference;
		//bool NeedsSaved;

		public Vector2 ChunkCoordinate;

		public readonly Dictionary<Vector2, ChunkCs> NeighbourChunks = new Dictionary<Vector2, ChunkCs>();
		

		static readonly Vector2 TextureAtlasSize = new Vector2(8, 4);
			
		static readonly float Sizex = 1.0f / TextureAtlasSize.X;
		static readonly float Sizey = 1.0f / TextureAtlasSize.Y;
		
		#if !Core
		static readonly SpatialMaterial Mat = (SpatialMaterial) GD.Load("res://assets/TextureAtlasMaterial.tres");

		readonly MeshInstance _blockMeshInstance = new MeshInstance();
		#endif


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

		public static readonly Vector3 Dimension = new Vector3(16, 384, 16);
		static readonly BaseGenerator Generator = new ForestGenerator();

		public byte[] BlockData = new byte[(int) Dimension.X * (int) Dimension.Y * (int) Dimension.Z];
		readonly bool[,,] _visibilityMask = new bool[(int) Dimension.X, (int) Dimension.Y, (int) Dimension.Z];
		public bool ChunkDirty;


		public void InstantiateChunk(ProcWorld w, int cx, int cz, long seed)
		{
			
			Pos = new Vector3(cx * (Dimension.X), 0, cz * Dimension.Z);
			ChunkCoordinate = new Vector2(cx, cz);
			
			Generator.Generate(this, cx, cz, seed);
			
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Update()
		{
			List<System.Numerics.Vector3> blocks = new();
			List<System.Numerics.Vector3> blocksNormals = new();
			List<Vector2>  uVs = new();

			//Making use of multidimensional arrays allocated on creation
			
			Stopwatch watch = Stopwatch.StartNew();
			for (int z = 0; z < Dimension.Z; z++)
			for (int y = 0; y < Dimension.Y; y++)
			for (int x = 0; x < Dimension.X; x++)
			{
				byte block = BlockData[GetFlattenedDimension(x, y, z)];
				if (block == 0) continue;
				bool[] check = check_transparent_neighbours(x, y, z);
				if (check.Contains(true))
				{
					_create_block(check, x, y, z, block, blocks, blocksNormals, uVs);	
				}
			}
			_chunkreference?.QueueDeletion();
			Mesh chunkmesh = new Mesh(blocks, uVs, this);


			chunkmesh.QueueVaoRegen();
			_chunkreference = chunkmesh;

			// Do Render stuff here
			ConsoleLibrary.DebugPrint(watch.ElapsedMilliseconds);

		}


		public void UpdateVisMask()
		{
			for (int z = 0; z < Dimension.Z; z++)
			for (int y = 0; y < Dimension.Y; y++)
			for (int x = 0; x < Dimension.X; x++)
			{
				_visibilityMask[x,y,z] = BlockHelper.BlockTypes[BlockData[GetFlattenedDimension(x,y,z)]].Transparent;
			}
		}
		
		// Called when the node enters the scene tree for the first time.
#if !Core
		public override void _Ready()
		{
			Mat.AlbedoTexture.Flags = 2;
			
			AddChild(_blockMeshInstance);		

		}
#endif

		public void _set_block_data(int x, int y, int z, byte b, bool overwrite = true)
		{
			if (x >= 0 && x < Dimension.X && y >= 0 && y < Dimension.Y && z >= 0 && z < Dimension.Z)
			{
				if (!overwrite && BlockData[GetFlattenedDimension(x, y, z)] != 0) return;
				BlockData[GetFlattenedDimension(x, y, z)] = b;

				_visibilityMask[x,y,z] = BlockHelper.BlockTypes[b].Transparent;
				ChunkDirty = true;
				//NeedsSaved = true;
			}
			else
			{
				//GD.Print("External Chunk Write");

				Vector3 worldCoordinates = new Vector3(x + Pos.X, y, z + Pos.Z);
				int localX = (int) (MathHelper.Modulo((float) Math.Floor(worldCoordinates.X), Dimension.X) + 0.5);
				int localY = (int) (MathHelper.Modulo((float) Math.Floor(worldCoordinates.Y), Dimension.Y) + 0.5);
				int localZ = (int) (MathHelper.Modulo((float) Math.Floor(worldCoordinates.Z), Dimension.Z) + 0.5);

				int cx = (int) Math.Floor(worldCoordinates.X / Dimension.X);
				int cz = (int) Math.Floor(worldCoordinates.Z / Dimension.Z);
				
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

		bool[] check_transparent_neighbours(int x, int y, int z)
		{
			return new[]
			{
				is_block_transparent(x, y + 1, z), is_block_transparent(x, y - 1, z), is_block_transparent(x - 1, y, z),
				is_block_transparent(x + 1, y, z), is_block_transparent(x, y, z - 1), is_block_transparent(x, y, z + 1)
			};
		}

		void _create_block(IReadOnlyList<bool> check, int x, int y, int z, byte block, List<System.Numerics.Vector3> blocks, List<System.Numerics.Vector3> blocksNormals, List<Vector2>  uVs)
		{
			List<BlockStruct> blockTypes = BlockHelper.BlockTypes;
			Vector3 coord = new Vector3(x, y, z);
			if (blockTypes[block].TagsList.Contains("Flat"))
			{
				Vector2 tempvar = blockTypes[block].Only;
				create_face(Cross1, ref coord, tempvar, blocks, blocksNormals, uVs);
				create_face(Cross2, ref coord, tempvar, blocks, blocksNormals, uVs);
				create_face(Cross3, ref coord, tempvar, blocks, blocksNormals, uVs);
				create_face(Cross4, ref coord, tempvar, blocks, blocksNormals, uVs);
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

		void create_face(IReadOnlyList<int> I, ref Vector3 offset, Vector2 textureAtlasOffset, List<System.Numerics.Vector3> blocks, List<System.Numerics.Vector3> blocksNormals, List<Vector2>  uVs)
		{
			System.Numerics.Vector3 a = V[I[0]] + offset;
			System.Numerics.Vector3 b = V[I[1]] + offset;
			System.Numerics.Vector3 c = V[I[2]] + offset;
			System.Numerics.Vector3 d = V[I[3]] + offset;



			Vector2 uvOffset = new Vector2(
				textureAtlasOffset.X / TextureAtlasSize.X,
				textureAtlasOffset.Y / TextureAtlasSize.Y
			);

			// the f means float, there is another type called double it defaults to that has better accuracy at the cost of being larger to store, but vector3 does not use it.
			Vector2 uvB = new Vector2(uvOffset.X, Sizey + uvOffset.Y);
			Vector2 uvC = new Vector2(Sizex, Sizey) + uvOffset;
			Vector2 uvD = new Vector2(Sizex + uvOffset.X, uvOffset.Y);


			blocks.AddRange(new[] {a, b, c, a, c, d});

			uVs.AddRange(new[] {uvOffset, uvB, uvC, uvOffset, uvC, uvD});

			//blocksNormals.AddRange(NormalGenerate(a, b, c, d));
		}
		
		static IEnumerable<Vector3> NormalGenerate(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			// HACK: Actually calculate normals as this only works for cubes

			Vector3 qr = c - a;
			Vector3 qs = b - a;

			Vector3 normal = new((qr.Y * qs.Z) - (qr.Z * qs.Y),(qr.Z * qs.X) - (qr.X * qs.Z), (qr.X * qs.Y) - (qr.Y * qs.X) );

			return new[] {normal, normal, normal, normal, normal, normal};

		}

		public static int GetFlattenedDimension(int x, int y, int z)
		{
			return x + y * (int)Dimension.Z + z * (int)Dimension.Y * (int)Dimension.X;
		}
		
		bool is_block_transparent(int x, int y, int z)
		{
			if (x < 0 || x >= Dimension.X || z < 0 || z >= Dimension.Z)
			{
				
				int cx = (int) Math.Floor(x / Dimension.X);
				int cz = (int) Math.Floor(z / Dimension.X);

				int bx = (int) (MathHelper.Modulo((float) Math.Floor((double) x), Dimension.X));
				int bz = (int) (MathHelper.Modulo((float) Math.Floor((double) z), Dimension.X));

				Vector2 cpos = new Vector2(cx, cz);


				if (ProcWorld.Instance.LoadedChunks.ContainsKey(cpos))
				{
					return ProcWorld.Instance.LoadedChunks[cpos]._visibilityMask[bx, y, bz];
				}
				return true;
			}

			if (y < 0 || y >= Dimension.Y)
			{
				return false;	
			}

			return _visibilityMask[x,y,z];
		}
		
		protected override void OnFree()
		{
			base.OnFree();
			_chunkreference?.QueueDeletion();
		}
	}
}
