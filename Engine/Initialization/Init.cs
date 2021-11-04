using Engine.Windowing;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Initialization
{
    public class Init
    {
        public static void InitEngine(int x, int y, int width, int height, string WindowName, Game gameclass)
        {
            WindowClass window = InitGame(x, y, width, height, WindowName, gameclass);
            WindowClass.Handle.Run();
            
        }

        static WindowClass InitGame(int x, int y, int width, int height, string WindowName, Game gameclass)
        {
            WindowClass window = new WindowClass(width, height,x,y,WindowName, gameclass);

            return window;
        }
    }
}