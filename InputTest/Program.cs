using System;
using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Input;
using Engine.Objects;
using Engine.Utilities.MathLib;

namespace InputTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            WindowParams windowParams = new WindowParams()
            {
                Size = new Int2(800, 600),
                Location = new Int2(100, 100),
                Name = "Default window",
            };
            Init.InitEngine(ref windowParams, new GameClass());
        }
    }

    internal class GameClass : GameEntry
    {

        BaseLevel _level;
        protected override void GameStart()
        {
            Console.WriteLine("Game started");
            base.GameStart();
            InputObject game = new InputObject();

            _level = new BaseLevel();
            
            _level.AddEntityToLevel(game);
            _level.Ticks = true;

        }
    }

    internal class InputObject : EngineObject
    {
        internal InputObject()
        {
            Ticks = true;
        }

        protected override void _Process(double delta)
        {
            Console.WriteLine("Tick");
            base._Process(delta);
            if (InputHandler.MouseDelta() != Int2.Zero)
            {
                Console.WriteLine(InputHandler.MousePos());
            }
        }
    }
}