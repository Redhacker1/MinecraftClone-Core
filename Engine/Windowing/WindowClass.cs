using System;
using Engine.Input;
using Engine.Rendering.Veldrid;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Engine.Windowing
{
    public class WindowClass
    {
        public static Renderer Renderer { get; protected set; }

        readonly GameEntry _gameInstance;
        public static IWindow Handle;

        public WindowClass(WindowOptions options, GameEntry GameClass)
        {
            //options.TransparentFramebuffer = true;
            Handle = Window.Create(options);
            Handle.IsContextControlDisabled = true;
            Handle.Closing += OnClose;
            Handle.Load += OnLoad;

            _gameInstance = GameClass;
        }
        
        public WindowClass(IWindow windowHandle, GameEntry gameClass)
        {
            
            Handle = windowHandle;
            Handle.Closing += OnClose;
            Handle.Load += OnLoad;

            _gameInstance = gameClass;
        }
        

        void OnLoad()
        {
            IInputContext context = Handle.CreateInput();
            InputHandler.InitInputHandler(context);

            Renderer = new Renderer(Handle);
            
            //Assign events.
            Handle.FramebufferResize += Renderer.OnResize;
            Handle.Render += Renderer.OnRender;
            Handle.Update += _gameInstance.Update;

            _gameInstance.GameStart();
            
        }

        void OnClose()
        {
            Renderer.Dispose();
            _gameInstance.GameEnded();
            Console.WriteLine("Closed!");
            Environment.ExitCode = 0;
        }
    }
}