/* TODO: This is starting to become a SuperClass with catch-all functionality, might be best to separate it out.
	Might be best to move some of the more chunk oriented methods into the chunkCS class that do not use the chunk class statically.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Engine.Debug;
using Engine.MathLib;
using Engine.Rendering;
using Engine.Windowing;
using MCClone_Core.Physics;
using MCClone_Core.Utility.IO;
using MCClone_Core.Utility.Threading;
using MCClone_Core.World_CS.Blocks;
using Veldrid;
using Random = Engine.MathLib.Random.Random;
using Shader = Engine.Rendering.Shader;
using Texture = Engine.Rendering.Texture;

namespace MCClone_Core.World_CS.Generation
{
	public class ProcWorld : Level
	{
		public bool UseThreadPool = true;
		int _chunksPerFrame = 1;

		public static bool Threaded = true;

		bool ForceRenderDistanceCheck;

		public static ProcWorld Instance;

		readonly ThreadPoolClass _threads = new ThreadPoolClass();
		
		// Max chunks radius comes out to (_loadRadius*2)^2 
		public int _loadRadius = 40;

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
		public Material _material;
		static Texture atlas;

		public ProcWorld(long seed)
		{
			Ticks = true;
			PhysicsTick = true;
			WorldSeed = seed;
			WorldRandom = new Random(seed); 
		}

		struct AtlasInfo
		{
			public uint length;
			public uint width;
			public Int2 TextureSize;
		}

		public override void _Ready()
		{
			if (Instance != null)
				return;
			Instance = this;
			
			MaterialDescription materialDescription = new MaterialDescription
			{
				BlendState = BlendStateDescription.SingleOverrideBlend,
				ComparisonKind = ComparisonKind.LessEqual,
				CullMode = FaceCullMode.Back,
				Topology = PrimitiveTopology.TriangleList,
				DepthTest = true,
				WriteDepthBuffer = true,
				FaceDir = FrontFace.Clockwise,
				FillMode = PolygonFillMode.Solid,
				Shaders = new Dictionary<ShaderStages, Shader>
				{
					{
						ShaderStages.Fragment,
						new Shader("./Assets/light_frag.spv", WindowClass._renderer.Device, ShaderStages.Fragment)
					},
					{
						ShaderStages.Vertex,
						new Shader("./Assets/intvert.spv", WindowClass._renderer.Device, ShaderStages.Vertex)
					}


				}
			};

			ResourceLayoutDescription vertexLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));

			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ModelBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("AtlasBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("Lighting", ResourceKind.UniformBuffer, ShaderStages.Fragment));
			
			_material = new Material(materialDescription, 
				new VertexLayoutDescription(
					new VertexElementDescription("PositionX", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1),
					new VertexElementDescription("PositionY", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1),
					new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)),
				WindowClass._renderer,
				vertexLayout,
				fragmentLayout
			);
			
			atlas = new Texture(WindowClass._renderer.Device, @"Assets\TextureAtlas.tga");
			var pointSampler = new TextureSampler(WindowClass._renderer.Device.PointSampler);

			AtlasInfo info = new AtlasInfo()
			{
				TextureSize = new Int2((int)atlas._texture.Width, (int)atlas._texture.Height),
				length = 8,
				width = 4,
			};
			Span<AtlasInfo> info_1 = stackalloc AtlasInfo[1];
			info_1[0] = info;

			Span<Vector4> Light = stackalloc Vector4[1];
			Light[0] = new Vector4(1,1,1,0);

			
			UniformBuffer<Vector4> AmbientLight = new UniformBuffer<Vector4>(WindowClass._renderer.Device, Light);
			UniformBuffer<AtlasInfo> buffer = new UniformBuffer<AtlasInfo>(WindowClass._renderer.Device, info_1);
			_material.ResourceSet(0, WindowClass._renderer.ViewProjBuffer);
			_material.ResourceSet(1, WindowClass._renderer.WorldBuffer, buffer, pointSampler, atlas, AmbientLight);



			ConsoleLibrary.DebugPrint("Starting procworld");
			
			ConsoleLibrary.DebugPrint("Preparing Threadpool");
			// Starts the threadpool;
			_threads.InitializePool();

			ConsoleLibrary.DebugPrint("Registering Blocks");
			// Sets the blocks used in the base game up.
			BlockHelper.RegisterBaseBlocks();
			
			ConsoleLibrary.DebugPrint("Creating Terrain Gen thread");
			// Preparing static terrain thread 
			_terrainThread = new Thread(_thread_gen);
			if (Threaded)
			{
				_terrainThread.Start();	
			}

			ConsoleLibrary.DebugPrint("Binding Console Commands");
			// Console Binds
			ConsoleLibrary.BindCommand("reload_chunks", "reloads all currently loaded chunks", "reload_chunks", ReloadChunks, false);
			ConsoleLibrary.BindCommand("reset", "Reloads world after saving, ","reset", Restart, false);
			
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!Threaded)
			{
				for (int i = 0; i < _chunksPerFrame; i++)
				{
					GenerationProcess();	
				}
			}
		}

		public void UpdateRenderDistance(int distance)
		{
			_loadRadius = distance;
			ForceRenderDistanceCheck = true;
		}


        private bool playerPosUpdated;

		void GenerationProcess()
		{
			playerPosUpdated = _newChunkPos != _chunkPos;

			_chunkPos = _newChunkPos;

			_currentChunkPos = _newChunkPos;

			if (ForceRenderDistanceCheck)
			{
				enforce_render_distance(_currentChunkPos);
				ForceRenderDistanceCheck = false;
			}
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
					if ((Math.Abs(deltaPos.Y - _currentLoadRadius) < 0.001f) && (Math.Abs(-deltaPos.X - _currentLoadRadius) > 0.001f))
					{
						//Go right
						_lastChunk = _load_chunk((int)_lastChunk.X - 1, (int) _lastChunk.Y);
					}
					// Either moving in constant x or we just reached bottom right. Addendum by donovan: this looping on the X axis has happened to me actually
					else if ((MathF.Abs(-deltaPos.X - _currentLoadRadius) < 0.001f) || (MathF.Abs(-deltaPos.X - deltaPos.Y) < 0.001f))
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
					if ((Math.Abs(-deltaPos.Y - _currentLoadRadius) < 0.001f) && (Math.Abs(deltaPos.X - _currentLoadRadius) > 0.001f))
					{
						//Go left
						_lastChunk = _load_chunk((int) _lastChunk.X + 1, (int) _lastChunk.Y);	
					}
					else if ((MathF.Abs(deltaPos.X - _currentLoadRadius) < 0.001f) || (MathF.Abs(deltaPos.X - (-deltaPos.Y)) < 0.001f))
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
		
		

		void _thread_gen()
		{
			ConsoleLibrary.DebugPrint("ThreadGen Thread Running");
			while (!_bKillThread)
			{
				GenerationProcess();
			}
		}
		Vector2 _load_chunk(int cx, int cz)
		{
			Int2 cpos = new Int2(cx, cz);
			bool loadChunk;
			loadChunk = !LoadedChunks.ContainsKey(new Vector2(cpos.X, cpos.Y));

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
				
				LoadedChunks[new Vector2(cpos.X, cpos.Y)] = c;
				c.UpdateVisMask();
				_update_chunk(cx, cz);
			}
			
			return new Vector2(cpos.X, cpos.Y);
		}

		public string ReloadChunks(params string[] args)
		{
			ICollection<Vector2> chunks = LoadedChunks.Keys;

			foreach (Vector2 chunkPos in chunks)
			{
				update_player_pos(chunkPos);
			}
			return $"{chunks.Count} Chunks sent to threadpool for processing...";
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
			
			int cx = x / ChunkCs.MaxX;
			int cz = z / ChunkCs.MaxX;

			int bx = (int) (MathHelper.Modulo((float) Math.Floor((double)x), ChunkCs.MaxX));
			int bz = (int) (MathHelper.Modulo((float) Math.Floor((double)z), ChunkCs.MaxZ));

			Vector2 chunkpos = new Vector2(cx, cz);

			if (LoadedChunks.ContainsKey(chunkpos) && ValidPlace(bx, y, bz))
			{
				return LoadedChunks[chunkpos].BlockData[ChunkCs.GetFlattenedIndex(bx, y, bz)];
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
			if (x < 0 || x >= ChunkCs.MaxX || z < 0 || z >= ChunkCs.MaxZ)
			{
				return false;
			}

			if(y < 0 || y > ChunkCs.MaxY - 1)
			{
				return false;
			}

			return true;
		}
		
		
		

		public void change_block(int cx, int cz, int bx, int by, int bz, byte T)
		{
			ChunkCs c = LoadedChunks[new Vector2(cx, cz)];

			if (c.BlockData[ChunkCs.GetFlattenedIndex(bx, by, bz)] == T) return;
			ConsoleLibrary.DebugPrint($"Changed block at {bx} {by} {bz} in chunk {cx}, {cz}");
			c?._set_block_data(bx,by,bz,T);
			_update_chunk(cx, cz);
		}

		void _update_chunk(int cx, int cz)
		{

			if (Threaded && UseThreadPool)
			{
				if (_threads.AllThreadsIdle() && _threads.Started == false)
				{
					_threads.IgniteThreadPool();
				}
				_threads.AddRequest(() =>
				{
					Vector2 cpos = new Vector2(cx, cz);
					if (LoadedChunks.ContainsKey(cpos))
					{
						LoadedChunks[cpos]?.Update();
					}

					return null;
				});	
			}
			else
			{
				if (_threads.Started && _threads.AllThreadsIdle())
				{
					_threads.ShutDownHandler();
				}
				Vector2 cpos = new Vector2(cx, cz);
				if (LoadedChunks.ContainsKey(cpos))
				{
					LoadedChunks[cpos]?.Update();
				}
			}
		}

		void enforce_render_distance(Vector2 currentChunkPos)
		{


			foreach (var key in LoadedChunks.Keys)
			{
				if (Math.Abs(key.X - currentChunkPos.X) > _loadRadius || Math.Abs(key.Y - currentChunkPos.Y) > _loadRadius)
					_unloadChunk((int) key.X, (int) key.Y);
			}
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
		}
	}
}
