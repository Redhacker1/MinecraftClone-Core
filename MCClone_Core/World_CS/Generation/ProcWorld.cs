/* TODO: This is starting to become a SuperClass with catch-all functionality, might be best to separate it out.
	Might be best to move some of the more chunk oriented methods into the chunkCS class that do not use the chunk class statically.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Engine.Attributes;
using Engine.Collision.Simple;
using Engine.Debugging;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering.Abstract;
using Engine.Rendering.VeldridBackend;
using Engine.Utilities.MathLib;
using MCClone_Core.Utility.IO;
using MCClone_Core.Utility.Threading;
using MCClone_Core.World_CS.Blocks;
using Veldrid;
using Material = Engine.Rendering.VeldridBackend.Material;
using Texture = Engine.Rendering.VeldridBackend.Texture;

namespace MCClone_Core.World_CS.Generation
{
	public class ProcWorld : BaseLevel
	{
		public static readonly ChunkMesher Mesher = new ChunkMesher();
		
		public bool UseThreadPool = true;
		int _chunksPerFrame = 8;

		public static bool Threaded = true;

		public static ProcWorld Instance;

		readonly ThreadPoolClass _threads = new ThreadPoolClass();
		
		// Max chunks radius comes out to (_loadRadius*2)^2 
		public int _loadRadius = 35;

		public readonly Random.Random WorldRandom;
		public readonly long WorldSeed;
		
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


		public static readonly IndexBuffer<uint> MasterIndexBuffer = new IndexBuffer<uint>(Engine.Engine.Renderer.Device,
			((ChunkCs.MaxX * ChunkCs.MaxY * ChunkCs.MaxZ) / 2) * 4 * 6);

		public ProcWorld(long seed)
		{
		
			Ticks = true;
			PhysicsTick = true;
			WorldSeed = seed;
			WorldRandom = new Random.Random(seed);
			ChunkCs.LoadedChunks = LoadedChunks;
		}

		public override void _Ready()
		{
			if (Instance != null)
				return;
			Instance = this;
			List<uint> indices = new List<uint>();

			for (uint i = 0; i < MasterIndexBuffer.Length; i += 4)
			{
				indices.Add(i);
				indices.Add(i + 1);
				indices.Add(i + 2);
				
				indices.Add(i);
				indices.Add(i + 2);
				indices.Add(i + 3);
			}
			
			MasterIndexBuffer.ModifyBuffer(indices.ToArray(), Engine.Engine.Renderer.Device);
			
			
			
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
				Shaders = new ShaderSet(
					ShaderExtensions.CreateShaderFromFile(ShaderType.Vertex, "./Assets/IntVertShader.vert", "main", ShaderExtensions.ShadingLanguage.GLSL),
					ShaderExtensions.CreateShaderFromFile(ShaderType.Fragment, "./Assets/shader.frag", "main", ShaderExtensions.ShadingLanguage.GLSL)
					)
			};

			ResourceLayoutDescription ProjectionLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));




			ResourceLayoutDescription fragmentLayout = new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly,
					ShaderStages.Fragment));

			VertexLayoutDescription[] vertexlayout = new[]
			{
				new VertexLayoutDescription((uint)Unsafe.SizeOf<Matrix4x4>(), 1, 
					new VertexElementDescription("Matrix1xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
					new VertexElementDescription("Matrix2xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
					new VertexElementDescription("Matrix3xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
					new VertexElementDescription("Matrix4xx", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)),
				new VertexLayoutDescription(12, 0,
					new VertexElementDescription("PositionXYZ", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1),
					new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
				)
			};

			_material = new Material(materialDescription, vertexlayout, Engine.Engine.Renderer,
				new[] {ProjectionLayout, fragmentLayout});
			
			atlas = new Texture(Engine.Engine.Renderer.Device, @"Assets\TextureAtlas.tga");
			TextureSampler pointSampler = new TextureSampler(Engine.Engine.Renderer.Device.PointSampler);
			_material.ResourceSet(1, pointSampler, atlas);



			ConsoleLibrary.DebugPrint("Starting procworld");
			
			ConsoleLibrary.DebugPrint("Preparing Threadpool");
			// Starts the thread-pool;
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
			
			Mesher.Start();

			ConsoleLibrary.DebugPrint("Binding Console Commands");
			// Console Binds
			ConsoleLibrary.BindCommand("reload_chunks", "reloads all currently loaded chunks", "reload_chunks", ReloadChunks);
			ConsoleLibrary.BindCommand("reset", "Reloads world after saving, ","reset", Restart);
			ConsoleLibrary.BindCommand("GCCall", "Runs the GC, ","GCCall", GCCall);
			
		}

		protected override void _Process(double delta)
		{
			base._Process(delta);
			if (!Threaded)
			{
				for (int i = 0; i < _chunksPerFrame; i++)
				{
					GenerationProcess();	
				}
			}
		}

		// This is a template ConCommand Attribute explantation for later
		[ConCommand("GCCall", "Runs the GC", "GCCall")]
		static string GCCall(params string[] args)
		{
			GC.Collect(2, GCCollectionMode.Forced, true, true);
			return string.Empty;
		}

		public void UpdateRenderDistance(int distance)
		{
			_loadRadius = distance;
			enforce_render_distance(_currentChunkPos);
		}


        private bool playerPosUpdated;

		void GenerationProcess()
		{
			
			playerPosUpdated = _newChunkPos != _chunkPos;
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
					if (Math.Abs(deltaPos.Y - _currentLoadRadius) < 0.001f && Math.Abs(-deltaPos.X - _currentLoadRadius) > 0.001f)
					{
						//Go right
						_lastChunk = _load_chunk((int)_lastChunk.X - 1, (int) _lastChunk.Y);
					}
					// Either moving in constant x or we just reached bottom right. Addendum by donovan: this looping on the X axis has happened to me actually
					else if (MathF.Abs(-deltaPos.X - _currentLoadRadius) < 0.001f || MathF.Abs(-deltaPos.X - deltaPos.Y) < 0.001f)
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
					if (Math.Abs(-deltaPos.Y - _currentLoadRadius) < 0.001f && Math.Abs(deltaPos.X - _currentLoadRadius) > 0.001f)
					{
						//Go left
						_lastChunk = _load_chunk((int) _lastChunk.X + 1, (int) _lastChunk.Y);	
					}
					else if (MathF.Abs(deltaPos.X - _currentLoadRadius) < 0.001f || MathF.Abs(deltaPos.X - -deltaPos.Y) < 0.001f)
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
			bool loadChunk = !LoadedChunks.ContainsKey(new Vector2(cpos.X, cpos.Y));
			
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
					c.InstantiateChunk(cx, cz, WorldSeed);	
				}
				
				LoadedChunks[new Vector2(cpos.X, cpos.Y)] = c;
				c.UpdateVisMask();
				_update_chunk(cx, cz);
				AddChild(c);

			}

			return new Vector2(cpos.X, cpos.Y);
		}

		public string ReloadChunks(params string[] args)
		{
			ICollection<ChunkCs> chunks = LoadedChunks.Values;

			foreach (ChunkCs chunk in chunks)
			{
				Mesher.AddMesh(chunk);
			}
			
			return $"Reloading {chunks.Count}";
		}
		

		public List<AABB> GetAabbs(int collisionlayer, Entity ent)
		{
			List<AABB> aabbs = new List<AABB>();
			AABB aabb = ent.bbox;
			aabb.GetMinMax(out Vector3 MinLoc, out Vector3 MaxLoc);
			
			Vector3 a = new Vector3(MinLoc.X - 1, MinLoc.Y - 1, MinLoc.Z - 1);

			for (int z = (int) a.Z; z < MaxLoc.Z; z++)
			{
				for (int y = (int) a.Y; y < MaxLoc.Y; y++)
				{
					for (int x = (int) a.X; x < MaxLoc.X; x++)
					{
						byte block = GetBlockIdFromWorldPos(x, y, z);
						if (BlockHelper.BlockTypes[block].NoCollision || block == 0) continue;
						AABB c = new AABB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));
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

			int bx = MathHelper.Modulo(x, ChunkCs.MaxX);
			int bz = MathHelper.Modulo(z, ChunkCs.MaxZ);

			Vector2 chunkPos = new Vector2(cx, cz);

			if (LoadedChunks.ContainsKey(chunkPos) && ValidPlace(bx, y, bz))
			{
				return LoadedChunks[chunkPos].BlockData.FullSpan[ChunkCs.GetFlattenedIndex(bx, y, bz)];
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
			return x >= 0 && x < ChunkCs.MaxX
		                && z >= 0 && z < ChunkCs.MaxZ
		                && y >= 0 && y <= ChunkCs.MaxY - 1;
		}
		
		
		

		public void change_block(int cx, int cz, int bx, int by, int bz, byte T)
		{
			if (!LoadedChunks.ContainsKey(new Vector2(cx, cz)))
			{
				_load_chunk(cx, cz);
			}
			ChunkCs c = LoadedChunks[new Vector2(cx, cz)];

			if (c.BlockData.FullSpan[ChunkCs.GetFlattenedIndex(bx, by, bz)] == T) return;
			ConsoleLibrary.DebugPrint($"Changed block at {bx} {by} {bz} in chunk {cx}, {cz}");
			c._set_block_data(bx,by,bz,T);
			_update_chunk(cx, cz);
		}

		void _update_chunk(int cx, int cz)
		{
			Vector2 cpos = new Vector2(cx, cz);
			if (LoadedChunks.ContainsKey(cpos))
			{
				Mesher.AddMesh(LoadedChunks[cpos]);
			}
		}

		void enforce_render_distance(Vector2 currentChunkPos)
		{
			foreach (Vector2 key in LoadedChunks.Keys)
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
				ChunkCs chunk = LoadedChunks[cpos];
	
				LoadedChunks.TryRemove(cpos, out _);
				RemoveChild(chunk);
			}

		}

		
		public void update_player_pos(Vector2 newPos)
		{
			_newChunkPos = newPos;
			playerPosUpdated = _newChunkPos != _chunkPos;
			_currentChunkPos = _newChunkPos;

		}

		void kill_thread()
		{
			_bKillThread = true;
			
			_threads.ShutDownHandler();
			Console.WriteLine("Threads shut down!");
		}

		string Restart(params string[] parameters)
		{
			SaveAndQuit();

			ProcWorld.Instance = new ProcWorld(this.WorldSeed);

			return "Restarting...";
			
		}
		
		public void SaveAndQuit()
		{
			Console.WriteLine("Save And Quit!");
			Mesher.Stop();
		#if !Core
			var tree = GetTree();
			if (tree != null)
			{
				tree.Paused = true;	
			}
		#endif


			Console.WriteLine("Saving Chunks");


			foreach (KeyValuePair<Vector2, ChunkCs> chunk in LoadedChunks)
			{
				if (chunk.Value.ChunkDirty)
				{
					_threads.AddRequest(() =>
					{
						SaveFileHandler.WriteChunkData(chunk.Value.BlockData.FullSpan,
							chunk.Value.ChunkCoordinate, World);
						RemoveChild(chunk.Value);

						return null; 
					});	
				}
			}
			
			Console.WriteLine("All chunks saved!");
	            
	            
			// Hack: this needs to be corrected, probably doable with a monitor.Lock() and then a callback to evaluate the END
			while (_threads.AllThreadsIdle() != true)
			{
		            
			}
			
			Console.WriteLine("All threads are idle!");
			kill_thread();
		}
		
	}
}
