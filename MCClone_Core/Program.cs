using System;
using System.Numerics;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;
using Engine;
using Engine.Initialization;
using Engine.Rendering.Windowing;
using MCClone_Core.Debug_and_Logging;
using MCClone_Core.Player_CS;
using MCClone_Core.Utility;
using MCClone_Core.Utility.IO;
using MCClone_Core.World_CS.Generation;

namespace MCClone_Core
{
    class Program
    {
        static WindowClass _window;

        static void Main(string[] args)
        {

            Init.InitEngine( 10,10, 1600, 900, "Hello World", new MinecraftCloneCore());

        }
    }

    internal class MinecraftCloneCore: Game
    {
        ProcWorld _world;
        public override void Gamestart()
        {
            base.Gamestart();
            WorldManager.FindWorlds();
            WorldData worldPath = WorldManager.CreateWorld();
            _world = new ProcWorld(1337) {World = worldPath};
            
            Player player = new Player(new Vector3( 0, 50, 0), Vector2.Zero, _world);
            player.Noclip = true;
            WorldScript script = new WorldScript(_world);
            script.Player = player;
            
            ConsoleLibrary.InitConsole(o => { Console.WriteLine(o.ToString()); });   
        }

        public override void GameEnded()
        {
            base.GameEnded();
            _world.SaveAndQuit();
        }
    }
}