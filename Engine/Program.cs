using System;
using System.Numerics;
using Engine.Initialization;
using Engine.Input;
using Engine.Objects;
using Silk.NET.Input;
using Veldrid;

namespace Engine
{
    public class Program
    {
        static void Main()
        {
            Init.InitEngine(0,0, 1024,768, "BaseEngine", new GameTest());
        }
    }

    internal class GameTest : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();
            var Entity = new UITestEntity();
        }
    }

    class UITestEntity : Entity
    {
    }
}