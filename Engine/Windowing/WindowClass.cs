using System;
using Engine.Input;
using Engine.Rendering.Veldrid;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Engine.Windowing
{
    public class WindowClass
    {
        [Obsolete]
        public static Renderer Renderer { get; protected set; }
        
        [Obsolete]
        readonly GameEntry _gameInstance;
        public static IWindow Handle;

        public WindowClass(WindowOptions options, GameEntry GameClass)
        {
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
            
            Engine.Renderer = new Renderer(Handle);
            Engine.MainFrameBuffer = new WindowRenderTarget(Engine.Renderer.Device);
            
            //Assign events.
            Handle.FramebufferResize += Engine.Renderer.OnResize;
            Handle.Render += Engine.Renderer.OnRender;
            Handle.Update += _gameInstance.Update;

            _gameInstance.GameStart();
            
        }

        void OnClose()
        {
            _gameInstance.GameEnded();
            Console.WriteLine("Closed!");
        }
    }
}