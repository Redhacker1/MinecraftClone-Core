using System;
using System.Diagnostics;
using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Objects;
using Engine.Windowing;
using MCClone_Core.Debug_and_Logging;
using MCClone_Core.Player_CS;
using MCClone_Core.Utility;
using MCClone_Core.Utility.IO;
using MCClone_Core.World_CS.Generation;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

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

            FPSEntity ent = new FPSEntity();
            WorldManager.FindWorlds();
            WorldData worldPath = WorldManager.CreateWorld();
            world = new ProcWorld(1337) {World = worldPath};
            
            Player player = new Player(new Vector3( 100 , 50, 100), Vector2.Zero, world);
            player.Noclip = false;
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
    
    
    
    class FPSEntity : Entity
    {
        Stopwatch fpstimer = Stopwatch.StartNew();
        int frames;

        public override void _Process(double delta)
        {
            Console.WriteLine($"Frames: {WindowClass.GetFPS()}");
        }
    }
}