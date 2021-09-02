using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Engine.Windowing
{
    public class VeldridWindow
    {
        Sdl2Window window;
        static GraphicsDevice _graphicsDevice;
        public void CreateWindow()
        {
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrid Tutorial"
            };
            window = VeldridStartup.CreateWindow(ref windowCI);
            
            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true
            };
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window,options);
        }
        
    }
}