using System;
using System.Diagnostics;
using System.Threading;
using Engine.Input;
using Engine.Rendering.VeldridBackend;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Windowing
{
    public class WindowClass
    {
        [Obsolete]
        public static Renderer Renderer { get; protected set; }

        static Thread _renderThread;
        
        public Action<Vector2D<int>> OnResize;
        


        [Obsolete]
        readonly GameEntry _gameInstance;
        public static IWindow Handle;

        public WindowClass(WindowOptions options, GameEntry gameClass)
        {
            Handle = Window.Create(options);
            Handle.IsContextControlDisabled = true;
            Handle.Closing += OnClose;
            Handle.Load += OnLoad;

            _gameInstance = gameClass;
        }
        
        public WindowClass(IWindow windowHandle, GameEntry gameClass)
        {
            
            Handle = windowHandle;
            Handle.Closing += OnClose;
            Handle.Load += OnLoad;

            _gameInstance = gameClass;
        }

        internal static bool IsRenderThread()
        {
            return Environment.CurrentManagedThreadId == _renderThread.ManagedThreadId;
        }
        

        void OnLoad()
        {
            IInputContext context = Handle.CreateInput();
            InputHandler.InitInputHandler(context);
            
            Engine.Renderer = new Renderer(Handle);
            Engine.MainFrameBuffer = new WindowRenderTarget(Engine.Renderer.Device);

            //Assign events.
            Handle.FramebufferResize += OnResize;
            OnResize += Engine.Renderer.Resize;

            _renderThread = new Thread(() =>
            {
                float deltaT = 0f;
                Stopwatch stopwatch = new Stopwatch();
                while (Handle.IsClosing != true)
                {
                    Engine.OnRender(deltaT);
                    deltaT = (float)stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Restart();
                }
            });
            Engine.OnRender += _gameInstance.OnRender;

            Handle.Update += _gameInstance.Update;
            _gameInstance.GameStart();
            _renderThread.Start();
            
        }

        void OnClose()
        {
            _gameInstance.GameEnded();
            Engine.Renderer.Dispose();
            Console.WriteLine("Closed!");
        }
    }
}