/* TODO: This is starting to become a SuperClass with catch-all functionality, might be best to separate it out.
	Might be best to move some of the more chunk oriented methods into the chunkCS class that do not use the chunk class statically.
 */

#if Core
	// Dependencies used in .net Core exclusively

using System.Threading;
	using System.Numerics;
	using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;
#else
	// Dependencies used in Godot standard library exclusively
	using Godot;

	using AABB = MinecraftClone.Utility.Physics.AABB;
	using MinecraftClone.World_CS.Utility;
	using MinecraftClone.Utility.CoreCompatibility;

	using Thread = System.Threading.Thread;
	using  Vector2 = System.Numerics.Vector2;
	using Vector3 = System.Numerics.Vector3;
#endif
	// Dependencies used Regardless
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
using Engine.MathLib;
using Engine.Objects;
using Engine.Physics;
using MinecraftClone.Debug_and_Logging;
using MinecraftClone.Utility.IO;
using MinecraftClone.Utility.Threading;
	using MinecraftClone.World_CS.Blocks;
	using Random = Engine.Random.Random;

namespace MinecraftClone.World_CS.Generation
{
	#if Core
	public class ProcWorld : Level
	#else
	public class ProcWorld : Spatial
	#endif
	{

		public static ProcWorld Instance;

		readonly ThreadPoolClass _threads = new ThreadPoolClass();
		
		// Max chunks radius comes out to (_loadRadius*2)^2 
		readonly int _loadRadius = 16;

		public static Random WorldRandom;
		public static long WorldSeed;
		
		public WorldData World;

		public readonly ConcurrentDictionary<Vector2, ChunkCs> LoadedChunks = new ConcurrentDictionary<Vector2, ChunkCs>();

		bool _bKillThread;
		Vector2 _chunkPos; 
		Vector2 _currentChunkPos;
		int _currentLoadRadius;
		Vector2 _lastChunk;

		Vector2 _newChunkPos;
		

		Thread _terrainThread;

		

		public ProcWorld(long seed)
		{
			WorldSeed = seed;
			WorldRandom = new Random(seed); 
		}
		
		protected override void _Ready()
		{
			if (Instance != null)
				return;
			Instance = this;

			ConsoleLibrary.DebugPrint("Starting procworld");
			
			ConsoleLibrary.DebugPrint("Preparing Threadpool");
			// Starts the threadpool;
			_threads.InitializePool(0);
			_threads.IgniteThreadPool();
			
			ConsoleLibrary.DebugPrint("Registering Blocks");
			// Sets the blocks used in the base game up.
			BlockHelper.RegisterBaseBlocks();
			
			ConsoleLibrary.DebugPrint("Creating Terrain Gen thread");
			// Preparing static terrain thread 
			_terrainThread = new Thread(_thread_gen);
			_terrainThread.Start();

			ConsoleLibrary.DebugPrint("Binding Console Commands");
			// Console Binds
			ConsoleLibrary.BindCommand("reload_chunks", "reloads all currently loaded chunks", "reload_chunks", ReloadChunks, false);
			ConsoleLibrary.BindCommand("reset", "Reloads world after saving, ","reset", Restart, false);
			
			
		}

		void _thread_gen()
		{
			ConsoleLibrary.DebugPrint("ThreadGen Thread Running");
			while (!_bKillThread)
			{
				bool playerPosUpdated = _newChunkPos != _chunkPos;

				_chunkPos = _newChunkPos;

				_currentChunkPos = _newChunkPos;

				if (playerPosUpdated)
				{
					enforce_render_distance(_currentChunkPos);
					_lastChunk = _load_chunk((int) _currentChunkPos.X, (int) _currentChunkPos.Y);
					_currentLoadRadius = 1;
				}
				else
				{
					// Load next chunk based on the position of the last one
					Vector2 deltaPos = _lastChunk - _currentChunkPos;
					// Only have player chunk
					if (deltaPos == Vector2.Zero)
					{
						// Move down one
						_lastChunk = _load_chunk((int) _lastChunk.X, (int) _lastChunk.Y + 1);
					}
					else if (deltaPos.X < deltaPos.Y)
					{
						// Either go right or up
						// Prioritize going right
						if ((deltaPos.Y == _currentLoadRadius) && (-deltaPos.X != _currentLoadRadius))
						{
							//Go right
							_lastChunk = _load_chunk((int)_lastChunk.X - 1, (int) _lastChunk.Y);
						}
						// Either moving in constant x or we just reached bottom right. Addendum by donovan: this looping on the X axis has happened to me actually
						else if ((-deltaPos.X == _currentLoadRadius) || (-deltaPos.X == deltaPos.Y))
						{
							// Go up
							_lastChunk = _load_chunk((int) _lastChunk.X, (int) _lastChunk.Y - 1);
						}
						else
						{
							// We increment here idk why
							if (_currentLoadRadius < _loadRadius)
							{
								_currentLoadRadius++;
							}
						}
					}
					else
					{
						//Either go left or down
						//Prioritize going left
						if ((-deltaPos.Y == _currentLoadRadius) && (deltaPos.X != _currentLoadRadius))
						{
							//Go left
							_lastChunk = _load_chunk((int) _lastChunk.X + 1, (int) _lastChunk.Y);	
						}
						else if ((deltaPos.X == _currentLoadRadius) || (deltaPos.X == -deltaPos.Y))
						{
							// Go down
							// Stop the last one where we'd go over the limit
							if (deltaPos.Y < _loadRadius)
							{
								_lastChunk = _load_chunk((int) _lastChunk.X, (int) _lastChunk.Y + 1);
							}
						}
					}
				}
			}
		}

		Vector2 _load_chunk(int cx, int cz)
		{
			Vector2 cpos = new Vector2(cx, cz);
			bool loadChunk;
			loadChunk = !LoadedChunks.ContainsKey(cpos);

			if (loadChunk)
			{
				ChunkCs c;
				if (SaveFileHandler.ChunkExists(World, cpos))
				{
					c = SaveFileHandler.GetChunkData(this ,World, cpos, out _);
				}
				else
				{
					c = new ChunkCs();
					c.InstantiateChunk(this, cx, cz, WorldSeed);	
				}
				
				LoadedChunks[cpos] = c;
				#if !Core
				if (c != null)
				{
					AddChild(c);
				}			
				#endif
				
				c?.UpdateVisMask();
				_update_chunk(cx, cz);
			}
			return cpos;
		}

		public string ReloadChunks(params string[] args)
		{
			IEnumerable<Vector2> chunks = LoadedChunks.Keys;

			foreach (Vector2 chunkPos in chunks)
			{
				update_player_pos(chunkPos);
			}
			return $"{chunks.Count()} Chunks sent to threadpool for processing...";
		}

		public override List<Aabb> GetAabbs(int collisionlayer, Aabb aabb)
		{
			List<Aabb> aabbs = new List<Aabb>();
			Vector3 a = new Vector3(aabb.MinLoc.X - 1, aabb.MinLoc.Y - 1, aabb.MinLoc.Z - 1);
			Vector3 b = aabb.MaxLoc;

			for (int z = (int) a.Z; z < b.Z; z++)
			{
				for (int y = (int) a.Y; y < b.Y; y++)
				{
					for (int x = (int) a.X; x < b.X; x++)
					{
						byte block = GetBlockIdFromWorldPos(x, y, z);
						if (BlockHelper.BlockTypes[block].NoCollision || block == 0) continue;
						Aabb c = new Aabb(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));
						aabbs.Add(c);
					}
				}
			}
			return aabbs;
		}


		void DebugLine(Vector3 a, Vector3 b)
		{
			#if !Core
				WorldScript.lines.Drawline(a.CastToGodot(), b.CastToGodot(), Colors.Red);
			#else
				throw new NotImplementedException();
			#endif

		}


		public byte GetBlockIdFromWorldPos(int x, int y, int z)
		{
			
			int cx = (int) Math.Floor(x / ChunkCs.Dimension.X);
			int cz = (int) Math.Floor(z / ChunkCs.Dimension.X);

			int bx = (int) (MathHelper.Modulo((float) Math.Floor((double)x), ChunkCs.Dimension.X));
			int bz = (int) (MathHelper.Modulo((float) Math.Floor((double)z), ChunkCs.Dimension.Z));

			Vector2 chunkpos = new Vector2(cx, cz);

			if (LoadedChunks.ContainsKey(chunkpos) && ValidPlace(bx, y, bz))
			{
				return LoadedChunks[chunkpos].BlockData[ChunkCs.GetFlattenedDimension(bx, y, bz)];
			}

			return 0;
		}
		
		
		/// <summary>
		/// Checks to ensure block is inside chunk, NOTE: Takes a relative position and does not account for validity of the chunk itself
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns>whether it is safe to write or read from the block in the chunk</returns>
		public static bool ValidPlace(int x, int y, int z)
		{
			if (x < 0 || x >= ChunkCs.Dimension.X || z < 0 || z >= ChunkCs.Dimension.Z)
			{
				return false;
			}

			if(y < 0 || y > ChunkCs.Dimension.Y - 1)
			{
				return false;
			}

			return true;
		}
		
		
		

		public void change_block(int cx, int cz, int bx, int @by, int bz, byte T)
		{
			ChunkCs c = LoadedChunks[new Vector2(cx, cz)];

			if (c.BlockData[ChunkCs.GetFlattenedDimension(bx, @by, bz)] == T) return;
			ConsoleLibrary.DebugPrint($"Changed block at {bx} {@by} {bz} in chunk {cx}, {cz}");
			c?._set_block_data(bx,@by,bz,T);
			_update_chunk(cx, cz);
		}

		void _update_chunk(int cx, int cz)
		{
			Vector2 cpos = new Vector2(cx, cz);

			_threads.AddRequest(() =>
			{
				if (LoadedChunks.ContainsKey(cpos))
				{
					LoadedChunks[cpos]?.Update();
				}

				return null;
			});
		}

		void enforce_render_distance(Vector2 currentChunkPos)
		{
			List<Vector2> keyList = new List<Vector2>(LoadedChunks.Keys);
			foreach (Vector2 location in keyList)
				if (Math.Abs(location.X - currentChunkPos.X) > _loadRadius ||
				    Math.Abs(location.Y - currentChunkPos.Y) > _loadRadius)
					_unloadChunk((int) location.X, (int) location.Y);
		}

		void _unloadChunk(int cx, int cz)
		{
			Vector2 cpos = new Vector2(cx, cz);
			if (LoadedChunks.ContainsKey(cpos))
			{
				SaveFileHandler.WriteChunkData(LoadedChunks[cpos].BlockData, LoadedChunks[cpos].ChunkCoordinate, World);
				
				LoadedChunks[cpos].Free();
			
				LoadedChunks.TryRemove(cpos, out _);
			}

		}

		public void update_player_pos(Vector2 newPos)
		{
			_newChunkPos = newPos;
		}

		void kill_thread()
		{
			_bKillThread = true;
			
			_threads.ShutDownHandler();
		}

		string Restart(params string[] parameters)
		{
			// Shuts down the old threadpool and saves the game state.
			SaveAndQuit();

			#if !Core
				if ( GetTree()  != null)
				{

					GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://Scenes/Spatial.tscn"));
				}
			#endif
			return "Restarting...";
			
		}
		
		public void SaveAndQuit()
		{
		#if !Core
			var tree = GetTree();
			if (tree != null)
			{
				tree.Paused = true;	
			}
		#endif


			ConsoleLibrary.DebugPrint("Saving Chunks");


			foreach (KeyValuePair<Vector2, ChunkCs> chunk in LoadedChunks)
			{
				if (chunk.Value.ChunkDirty)
				{
					_threads.AddRequest(() =>
					{
						SaveFileHandler.WriteChunkData(chunk.Value.BlockData,
							chunk.Value.ChunkCoordinate, World);
						chunk.Value.Free();
						

						return null; 
					});	
				}
			}
	            
	            
			// Hack: this needs to be corrected, probably doable with a monitor.Lock() and then a callback to evaluate the END
			while (_threads.AllThreadsIdle() != true)
			{
		            
			}
			kill_thread();

		#if !Core
			if (tree != null)
			{
				tree.Paused = false;
			}
		#endif
		}
	}
}
