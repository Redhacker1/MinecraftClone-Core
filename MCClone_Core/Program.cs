using System;
using System.Numerics;
using Engine;
using Engine.Attributes;
using Engine.Debugging;
using Engine.Initialization;
using Engine.Renderable;
using MCClone_Core.Player_CS;
using MCClone_Core.Utility;
using MCClone_Core.Utility.IO;
using MCClone_Core.World_CS.Generation;
using Steamworks;

namespace MCClone_Core
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Init.InitEngine( 10,10, 1600, 900, "Hello World", new MinecraftCloneCore());

        }
    }

    [GameDefinition("Blocky Worlds", true)]
    internal class MinecraftCloneCore: GameEntry
    {
        WorldScript script;
        ImGUIPanel _panel;
        ConsoleText consoleBox = new ConsoleText();
        ProcWorld world;
        Player player;

        protected override void GameStart()
        {
            base.GameStart();
            
            ConsoleLibrary.InitConsole(consoleBox.SetConsoleScrollback);
            Console.WriteLine("Initializing Steam API");
            try
            {
                SteamClient.Init(1789570);
                Console.WriteLine("SteamAPI enabled successfully");

            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load SteamAPI, some features may be unavailable");
            }


            WorldManager.FindWorlds();
            WorldData worldPath = WorldManager.CreateWorld();
            
            world = new ProcWorld(1337) {World = worldPath};
            script = new WorldScript(world);
            player = new Player(new Vector3( 0 , 50, 0), Vector2.Zero)
            {
                Noclip = false
            };
            
            PinnedObject = world;
            world.AddChild(script);
            world.AddChild(player);
            
            script._player = player;
            player.World = world;
            
            script._Ready();
            world._Ready();
            player._Ready();
            
            
            base.GameStart();
        }

        protected override void GameEnded()
        {
            Console.WriteLine("Shutting down SteamAPI");
            SteamClient.Shutdown();
            world.SaveAndQuit();
        }
        
        
        
    }
}