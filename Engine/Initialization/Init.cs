using Engine.Rendering.VeldridBackend;
using Engine.Rendering.Windowing;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Initialization
{
    public class Init
    {
        public static void InitEngine(int x, int y, int width, int height, string windowName, Game gameclass)
        {
            WindowClass window = InitGame(x, y, width, height, windowName, gameclass);
            window.Handle?.Run();
            
        }

        static WindowClass InitGame(int x, int y, int width, int height, string windowName, Game gameclass)
        {

            WindowOptions options = WindowOptions.Default;
            options.Size = new Vector2D<int>(width, height);
            options.Position = new Vector2D<int>(x, y);
            options.Title = windowName;
            options.VSync = false;
            options.API = GraphicsAPI.Default;
            
            IWindow handle = Window.Create(options);
            WindowClass window = new WindowClass(handle,gameclass, new VeldridRenderer(handle));

            return window;
        }
    }
}