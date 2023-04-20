using System;
using System.Diagnostics;
using System.Threading;
using Engine.Input;
using Engine.Rendering.VeldridBackend;
using Silk.NET.Maths;
using Veldrid.Sdl2;

namespace Engine.Windowing
{
    public class WindowEvents
    {
        static Thread _renderThread;
        
        public Action<Vector2D<int>> OnResize;
        


        [Obsolete]
        readonly GameEntry _gameInstance;
        public static Sdl2Window Handle;

        public WindowEvents(Sdl2Window windowHandle, GameEntry gameClass)
        {
            
            Handle = windowHandle;
            Handle.Closing += OnClose;

            _gameInstance = gameClass;
        }

        internal static bool IsRenderThread()
        {
            return Environment.CurrentManagedThreadId == _renderThread.ManagedThreadId;
        }
        
        
        void OnLoad()
        {
            InputHandler.InitInputHandler(Handle);
            
            Engine.Renderer = new Renderer(Handle);
            Engine.MainFrameBuffer = new WindowRenderTarget(Engine.Renderer.Device);

            //Assign events.
            Handle.Resized += () =>
            {
                OnResize(new Vector2D<int>(Handle.Width, Handle.Height));
            };
            
            OnResize += Renderer.Resize;

            _renderThread = new Thread(() =>
            {
                float deltaT = 0f;
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (Handle?.Exists == true)
                {
                    Engine.OnRender(deltaT);
                    deltaT = (float)stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Restart(); 
                }
            });
            
            Engine.OnRender += dt =>
            {
                Engine.Renderer.OnRenderHook();
                
                _gameInstance.OnRender(dt);
            };

            //Handle.Update += _gameInstance.Update;
            _gameInstance.GameStart();
            _renderThread.Start();
            
        }

        void Update(double delta)
        {
            _gameInstance.Update(delta);
        }


        bool closed = false;

        internal void Run()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            OnLoad();
            while (!closed)
            {
                var evnt = Handle.PumpEvents();
                Engine.CurrentInput = evnt;
                
                Update(stopwatch.Elapsed.Seconds);
                stopwatch.Restart();

            }
        }
        

        void OnClose()
        {
            _gameInstance.GameEnded();
            Engine.Renderer.Dispose();
            Console.WriteLine("Closed!");
            closed = true;
        }
    }
}