using MinecraftClone.Debug_and_Logging;
using MinecraftClone.Player_CS;
using MinecraftClone.Utility.IO;
using MinecraftClone.World_CS.Blocks;
using MinecraftClone.World_CS.Generation;
using MinecraftClone.World_CS.Utility.Debug;
using System;
using System.Numerics;
using Engine.Objects;

namespace MinecraftClone.Utility
{
	#if Core
	public class WorldScript : GameObject
	#else
	//[Tool]
	public class WorldScript : Node
	#endif
	{
		Vector2 _chunkPos;

		long _chunkX = 1;
		long _chunkZ = 1;
		public Player _player;

		public static DebugLines lines = new DebugLines();

		//public Logger Logger = new Logger(Path.Combine(Environment.CurrentDirectory,"Logs"), "DebugFile", ConsoleLibrary.DebugPrint);

		static public ProcWorld _pw;

		// Called when the node enters the scene tree for the first time.

		public WorldScript(ProcWorld world_data)
		{
			_pw = world_data;
		}

		protected override void _Ready()
		{
			BlockHelper.RegisterBaseBlocks();
			WorldManager.FindWorlds();

			
			#if !Core
			AddChild(_pw);
			Connect("tree_exiting", this, "_on_WorldScript_tree_exiting");
			AddChild(lines);
			_player = GetNode<Node>("Player") as Player;
			if (!Engine.EditorHint)
			{
				ConsoleLibrary.DebugPrint("CREATING WORLD");	
			}
			else
			{
				GD.Print("CREATING WORLD (Editor)");
			}
			#endif


			//_player.World = _pw;
			//_player.GameManager = this;
		}

		void _on_WorldScript_tree_exiting()
		{
			ConsoleLibrary.DebugPrint("Kill map loading thread");
			if (_pw != null)
			{
				_pw.SaveAndQuit();
				ConsoleLibrary.DebugPrint("Finished");
			}
		}
		

		//Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			//base._Process(delta);
			
			if (_player == null) return;
			_chunkX = ((long) Math.Floor(_player.Pos.X)) / ((int)ChunkCs.Dimension.X);
			_chunkZ = ((long) Math.Floor(_player.Pos.Z)) / ((int)ChunkCs.Dimension.Z);

			Vector2 newChunkPos = new Vector2(_chunkX, _chunkZ);

			if (newChunkPos == _chunkPos)
			{
				return;
			};
			//ConsoleLibrary.DebugPrint("Chunk Updated");
			_chunkPos = newChunkPos;
			_pw.update_player_pos(_chunkPos);
			ConsoleLibrary.DebugPrint($" X: {_chunkX}, Z: {_chunkZ}");
		}
	}
}
