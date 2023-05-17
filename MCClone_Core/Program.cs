using System;
using System.Numerics;
using Engine;
using Engine.Attributes;
using Engine.Debugging;
using Engine.Initialization;
using Engine.Objects.SceneSystem;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using Engine.Utilities.MathLib;
using MCClone_Core.Player_CS;
using MCClone_Core.Utility;
using MCClone_Core.Utility.IO;
using MCClone_Core.World_CS.Generation;
using Steamworks;

namespace MCClone_Core
{
    static class Program
    {
        static void Main(string[] args)
        {
            
            WindowParams windowParams = new WindowParams()
            {
                Location = new Int2(100, 100),
                Size = new Int2(1280, 720),
                Name = "Default window",
            };
            Init.InitEngine(windowParams, new MinecraftCloneCore());

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

        public static Scene Scene;
        

        protected override void GameStart()
        {
            
            Scene = new Scene();
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
            player = new Player(new Vector3( 0 , 200, 0), Vector2.Zero)
            {
                Noclip = false
            };
            
            PinnedObject = world;
            world.AddChild(script);
            world.AddChild(player);

            script._player = player;
            player.World = world;
            
            script._Ready();
            player._Ready();
            
            Scene.AddStage(new DefaultRenderPass(Engine.Engine.Renderer));


            base.GameStart();
            player.Position = new Vector3(0, 200, 0);
        }

        protected override void GameEnded()
        {
            Console.WriteLine("Shutting down SteamAPI");
            SteamClient.Shutdown();
            world.SaveAndQuit();
            Console.WriteLine($"Gamemode: {world.GetType().Name} Ended");
        }

        protected override void OnRender(float deltaT)
        {
            base.OnRender(deltaT);
            Scene?.Render(Camera.MainCamera, Engine.Engine.MainFrameBuffer, deltaT);
        }
    }
}