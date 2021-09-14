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
            Glfw GlfwHandle = Glfw.GetApi();

            WindowOptions options = WindowOptions.Default;
            options.Size = new Vector2D<int>(width, height);
            options.Position = new Vector2D<int>(x, y);
            options.Title = WindowName;
            options.VSync = false;
            
            IWindow handle = Window.Create(options);

            WindowClass window = new WindowClass(GlfwHandle, handle, gameclass);

            return window;
        }
    }
}