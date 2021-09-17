using System;
using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Input;
using Engine.Objects;

namespace InputTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Init.InitEngine(10, 10, 1024, 768, "InputTest", new GameClass());
        }
    }

    class GameClass : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();
            var game = new InputObject();
        }
    }

    class InputObject : GameObject
    {
        public override void _Process(double delta)
        {
            base._Process(delta);
            if (InputHandler.MouseDelta(0) != Vector2.Zero)
            {
                Console.WriteLine(InputHandler.MousePos(0));
            }
        }
    }
}