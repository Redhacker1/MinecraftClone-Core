using System;
using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;

namespace InputTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Init.InitEngine(10, 10, 1024, 768, "InputTest", new GameClass());
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
            if (InputHandler.MouseDelta(0) != Vector2.Zero)
            {
                Console.WriteLine(InputHandler.MousePos(0));
            }
        }
    }
}