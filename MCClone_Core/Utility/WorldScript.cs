using System;
using System.Numerics;
using Engine.Debugging;
using Engine.Objects;
using MCClone_Core.Debug_and_Logging;
using MCClone_Core.Player_CS;
using MCClone_Core.World_CS.Generation;

namespace MCClone_Core.Utility
{
	#if Core
	public class WorldScript : EngineObject
	#else
	//[Tool]
	public class WorldScript : Node
	#endif
	{
		Vector2 _chunkPos;

		int _chunkX = 1;
		int _chunkZ = 1;
		public Player _player;

		public static DebugLines lines = new DebugLines();

		//public Logger Logger = new Logger(Path.Combine(Environment.CurrentDirectory,"Logs"), "DebugFile", ConsoleLibrary.DebugPrint);

		readonly ProcWorld _pw;
		// Called when the node enters the scene tree for the first time.

		public WorldScript(ProcWorld worldData)
		{
			Ticks = true;
			PhysicsTick = true;
			_pw = worldData;
		}

		DebugPanel _debug;

		public override void _Ready()
		{
			_debug = new DebugPanel(_pw);


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


		//Called every frame. 'delta' is the elapsed time since the previous frame.
		protected override void _Process(double delta)
		{

			if (_player == null) return;
			_chunkX = (int) (Math.Floor(_player.Position.X) / ChunkCs.MaxX);
			_chunkZ = (int) (Math.Floor(_player.Position.Z) / ChunkCs.MaxZ);

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
