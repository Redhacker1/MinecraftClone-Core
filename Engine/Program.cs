using System.Numerics;
using Engine.Debugging;
using Engine.Initialization;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Abstract;

namespace Engine
{
    public class Program
    {
        

        static void Main(params string[] args)
        {
            // TODO: This is where engine 
            Init.InitEngine(0,0, 1024,768, "BaseEngine", new GameTest());
        }
    }

    internal class GameTest : GameEntry
    {
        UITestEntity Entity;
        public override void Gamestart()
        {
            base.Gamestart();
            Entity = new UITestEntity();
        }
    }

    class UITestEntity : Entity
    {
        ImGUIPanel console;

        public override void _Ready()
        {
            console = new ConsoleText();
        }
    }
}