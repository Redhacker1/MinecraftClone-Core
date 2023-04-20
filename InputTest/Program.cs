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
                Location = Int2.Zero,
                Size = new Int2(1920, 1080),
                Name = "Default window",
            };
            Init.InitEngine(ref windowParams, new GameClass());
        }
    }

    internal class GameClass : GameEntry
    {
        protected override void GameStart()
        {
            Console.WriteLine("Game started");
            base.GameStart();
            InputObject game = new InputObject();

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
            base._Process(delta);
            if (InputHandler.MouseDelta() != Vector2.Zero)
            {
                Console.WriteLine(InputHandler.MousePos());
            }
        }
    }
}