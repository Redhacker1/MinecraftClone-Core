// See https://aka.ms/new-console-template for more information
// https://vitaliburkov.wordpress.com/2016/09/17/simple-and-fast-high-quality-antialiased-lines-with-opengl/

using Engine;
using ObjDemo;

namespace LineTest
{
    static class Program
    {
        static void Main()
        {
            Engine.Initialization.Init.InitEngine(1024, 768, 1920, 1080, "TestLines", new LinesDemo());
        }
    }

    class LinesDemo : GameEntry
    {
        Player character;
        Line line;
        public override void Gamestart()
        {
            character = new Player();
            line = new Line();

            base.Gamestart();
        }
    }


}