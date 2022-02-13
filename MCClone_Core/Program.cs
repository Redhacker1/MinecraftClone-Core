using System;
using System.Diagnostics;
using System.Numerics;
using Engine;
using Engine.Debug;
using Engine.Initialization;
using Engine.Objects;
using Engine.Renderable;
using Engine.Windowing;
using MCClone_Core.Debug_and_Logging;
using MCClone_Core.Player_CS;
using MCClone_Core.Utility;
using MCClone_Core.Utility.IO;
using MCClone_Core.World_CS.Generation;
using Steamworks;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace MCClone_Core
{
    internal class Program
    {
        static WindowClass window;

        static void Main(string[] args)
        {

            Init.InitEngine( 10,10, 1600, 900, "Hello World", new MinecraftCloneCore());

        }
    }

    internal class MinecraftCloneCore: Game
    {
        WorldScript script;
        ImGUIPanel _panel = new ImGUIPanel();
        ConsoleText consoleBox = new ConsoleText();
        ProcWorld world;
        public override void Gamestart()
        {
            ConsoleLibrary.InitConsole(consoleBox.SetConsoleScrollback);
            Console.WriteLine("Initializing Steam API");
            try
            {
                SteamClient.Init(1789570);
                Console.WriteLine("SteamAPI enabled successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load SteamAPI, some features will be unavailable (soon)");
            }

            base.Gamestart();

            FPSEntity ent = new FPSEntity();
            WorldManager.FindWorlds();
            WorldData worldPath = WorldManager.CreateWorld();
            world = new ProcWorld(1337) {World = worldPath};
            
            Player player = new Player(new Vector3( 0 , 50, 0), Vector2.Zero, world);
            player.Noclip = false;
            script = new WorldScript(world);
            script._player = player;
            player.World = world;
        }

        public override void GameEnded()
        {
            Console.WriteLine("Shutting down SteamAPI");
            SteamClient.Shutdown();
            
            world.SaveAndQuit();
        }
        
        
        
    }


    internal class FPSEntity : Entity
    {
        uint frames;
        uint PreviousFPS = 0;

        public override void _Process(double delta)
        {
        }
    }
}