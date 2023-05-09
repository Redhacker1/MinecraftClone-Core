using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Engine.Input;
using Engine.Rendering.VeldridBackend;
using Engine.Utilities.MathLib;
using SharpInterop.SDL2;

namespace Engine.Windowing
{
    public class WindowEvents
    {
        static Thread _renderThread;

        Int2 Size;
        
        public Action<Int2> OnResize;
        


        [Obsolete]
        readonly GameEntry _gameInstance;
        public static IntPtr Handle;

        public WindowEvents(IntPtr windowHandle, GameEntry gameClass)
        {

            Handle = windowHandle;

            _gameInstance = gameClass;
        }

        internal static bool IsRenderThread()
        {
            return Environment.CurrentManagedThreadId == _renderThread.ManagedThreadId;
        }
        
        
        void OnLoad()
        {
            InputHandler.InitInputHandler(Handle);
            
            Engine.Renderer = new Renderer(Handle, Size);
            Engine.MainFrameBuffer = new WindowRenderTarget(Engine.Renderer.Device);

            //Assign events.

            OnResize += Renderer.Resize;

            _renderThread = new Thread(() =>
            {
                float deltaT = 0f;
                Stopwatch stopwatch = Stopwatch.StartNew();

                
                while (closed != true)
                {
                    Engine.OnRender(deltaT);
                    deltaT = (float)stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Restart(); 
                    Engine.Renderer.SwapBuffers();
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
            _gameInstance.Update((float)delta);
        }


        bool closed;

        internal void Run()
        {
            OnLoad();
            Stopwatch stopwatch = new Stopwatch();
            
            while (closed == false)
            {
                float deltaT = (float)stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();
                while (SDL.SDL_PollEvent(out SDL.SDL_Event sdlEvent) != (int) SDL.SDL_bool.SDL_FALSE)
                {
                    switch (sdlEvent.type)
                    {
                        case SDL.SDL_EventType.SDL_KEYUP:
                        {
                            OnKeyPressed(sdlEvent.key.keysym.sym, sdlEvent.key.keysym.mod,sdlEvent.key.repeat > 0);
                            break;
                        }
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                        {
                            break;
                        }
                        case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                        {
                            break;
                        }
                        case SDL.SDL_EventType.SDL_QUIT:
                        {
                            closed = true;
                            break;
                        }
                        case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        {
                            if (sdlEvent.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                            {
                                OnResize(new Int2(sdlEvent.window.data1, sdlEvent.window.data2));
                            }
                            break;
                        }
                        case SDL.SDL_EventType.SDL_TEXTINPUT

                    }


                    Update(deltaT);
                }
                OnClose();
            }
        }

        void OnKeyPressed(SDL.SDL_Keycode scancode, SDL.SDL_Keymod modifiers, bool repeat)
        {
            Keycode Enginekeycode = (Keycode) (int) scancode;
            KeyModifiers EngineModifiers = (KeyModifiers) (ushort) modifiers;
            char character;

            if ((uint) scancode > 32 && (uint) scancode < 128)
            {
                character = (char) scancode;   
            }
        }

        void OnKeyReleased(SDL.SDL_Keycode scancode)
        {
            
        }

        void OnMouseWheel()
        {
            
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