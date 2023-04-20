// See https://aka.ms/new-console-template for more information
// https://vitaliburkov.wordpress.com/2016/09/17/simple-and-fast-high-quality-antialiased-lines-with-opengl/

using Engine;
using Engine.Initialization;
using Engine.Utilities.MathLib;
using ObjDemo;

namespace LineTest
{
    static class Program
    {
        static void Main()
        {
            WindowParams windowParams = new WindowParams()
            {
                Location = Int2.Zero,
                Size = new Int2(1280, 720),
                Name = "Default window",
            };
            Engine.Initialization.Init.InitEngine(ref windowParams, new LinesDemo());
        }
    }

    class LinesDemo : GameEntry
    {
        Player character;
        Line line;
        protected override void GameStart()
        {
            character = new Player();
            line = new Line();

            base.GameStart();
        }
    }


}