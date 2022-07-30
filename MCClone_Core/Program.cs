using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Engine;
using Engine.Attributes;
using Engine.Debugging;
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

    [GameDefinition("Blocky Worlds", true)]
    internal class MinecraftCloneCore: GameEntry
    {
        WorldScript script;
        ImGUIPanel _panel;
        ConsoleText consoleBox = new ConsoleText();
        ProcWorld world;
        Player player;
        
        public override void Gamestart()
        {
            Stopwatch watch = Stopwatch.StartNew();
            List<Type> obsoleteTypes = AttributeHelpers.FindAllTypeOfAttribute<ObsoleteAttribute>();
            Console.WriteLine(obsoleteTypes.Count);
            Console.WriteLine("Classes with obsolete items found: ");
            foreach (Type type in obsoleteTypes)
            {
                Console.WriteLine($"Classname: {type.Name} is obsolete!");
                Attribute.GetCustomAttributes(type);

                AttributeHelpers.GetAttributes<ObsoleteAttribute>(type);
                
                IEnumerable<object> attributes = AttributeHelpers.GetAttributes(type).Where(attribute => attribute 
                    is ObsoleteAttribute);
                foreach (Attribute attribute in attributes)
                {
                    ObsoleteAttribute obsoleteAttribute = attribute as ObsoleteAttribute;
                    Console.WriteLine(obsoleteAttribute?.Message);
                }
            }

            Console.WriteLine(watch.ElapsedMilliseconds);
            
            
            ConsoleLibrary.InitConsole(consoleBox.SetConsoleScrollback);
            Console.WriteLine("Initializing Steam API");
            try
            {
                SteamClient.Init(1789570);
                Console.WriteLine("SteamAPI enabled successfully");
                IEnumerable<Friend> friends = SteamFriends.GetFriends();
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load SteamAPI, some features will be unavailable (soon)");
            }

            base.Gamestart();
            WorldManager.FindWorlds();
            WorldData worldPath = WorldManager.CreateWorld();
            world = new ProcWorld(1337) {World = worldPath};
            
            player = new Player(new Vector3( 0 , 50, 0), Vector2.Zero);
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