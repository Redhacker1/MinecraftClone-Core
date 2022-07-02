using System;
using System.Numerics;
using Engine;
using Engine.Debug;
using Engine.Initialization;
using Engine.Renderable;
using Engine.Windowing;
using MCClone_Core.Player_CS;
using MCClone_Core.Utility;
using MCClone_Core.Utility.IO;
using MCClone_Core.World_CS.Generation;
using Steamworks;

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
        ImGUIPanel _panel;
        ConsoleText consoleBox = new ConsoleText();
        ProcWorld world;
        Player player;
        
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
            WorldManager.FindWorlds();
            WorldData worldPath = WorldManager.CreateWorld();
            world = new ProcWorld(1337) {World = worldPath};
            
            player = new Player(new Vector3( 0 , 50, 0), Vector2.Zero, world);
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
}