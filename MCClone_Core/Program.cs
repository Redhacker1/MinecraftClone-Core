using System;
using System.Numerics;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;
using Engine;
using Engine.Initialization;
using Engine.Windowing;
using MinecraftClone.Debug_and_Logging;
using MinecraftClone.Player_CS;
using MinecraftClone.Utility;
using MinecraftClone.Utility.IO;
using MinecraftClone.World_CS.Generation;

namespace MCClone_Core
{
    class Program
    {
        static WindowClass window;

        static void Main(string[] args)
        {

            Init.InitEngine( 10,10, 1600, 900, "Hello World", new MinecraftCloneCore());

        }
    }

    internal class MinecraftCloneCore: Game
    {
        ProcWorld world;
        public override void Gamestart()
        {
            base.Gamestart();
            WorldManager.FindWorlds();
            WorldData worldPath = WorldManager.CreateWorld();
            world = new ProcWorld(1337) {World = worldPath};
            
            Player player = new Player(new Vector3( 40 , 50, 0), Vector2.Zero, world);
            player.Pos.Y = 100;
            WorldScript script = new WorldScript(world);
            script._player = player;
            
            ConsoleLibrary.InitConsole(o => { Console.WriteLine(o.ToString()); });   
        }

        public override void GameEnded()
        {
            base.GameEnded();
            world.SaveAndQuit();
        }
    }
}