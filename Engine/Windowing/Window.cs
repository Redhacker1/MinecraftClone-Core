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
    public class Window
    {
        static Thread _renderThread;
        MouseState CurrentMouseState = default;

        public static Int2 Size
        {
            get
            {
                int x, y;
                SDL.SDL_GetWindowSize(Handle, out x, out y);
                return new Int2(x, y);
            }
        }

        public Action<Int2> OnResize;
        


            [Obsolete]
            readonly GameEntry _gameInstance;
            static IntPtr Handle;

            public Window(IntPtr windowHandle, GameEntry gameClass)
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
                InputHandler.InitInputHandler(this);
                    
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


            bool closed = false;

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
                                OnKeyReleased(sdlEvent.key.keysym.sym);
                                break;
                            }
                            case SDL.SDL_EventType.SDL_KEYDOWN:
                            {
                                OnKeyPressed(sdlEvent.key.keysym.sym, sdlEvent.key.keysym.mod,sdlEvent.key.repeat > 0);
                                break;
                            }
                            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                            {
                                SDL.SDL_MouseMotionEvent mouseEvent  = sdlEvent.motion;
                                MouseMoved(mouseEvent.x, mouseEvent.y, mouseEvent.xrel, mouseEvent.y);
                                break;
                            }
                            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                            {
                                break;
                            }
                            case SDL.SDL_EventType.SDL_QUIT:
                            {
                                Console.WriteLine("Recieved Quit event");
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

                            default:
                                break;

                        }


                        Update(deltaT);
                    }
                }
                OnClose();
            }

            void OnKeyPressed(SDL.SDL_Keycode scancode, SDL.SDL_Keymod modifiers, bool repeat)
            {
                Keycode Enginekeycode = (Keycode) (int) scancode;
                KeyModifiers EngineModifiers = (KeyModifiers) (ushort) modifiers;
                char character = default;

                if ((uint) scancode >= 33 && (uint) scancode <= 126)
                {
                    character = (char) scancode;
                }

                KeyPressedEvent(Enginekeycode, EngineModifiers, character, repeat);
            }

            void OnKeyReleased(SDL.SDL_Keycode scancode)
            {
                Keycode Enginekeycode = (Keycode) (int) scancode;
                KeyReleasedEvent(Enginekeycode);
            }

            int hackForMouseY = 0;
            void MouseMoved(int X, int Y, int RelativeX, int RelativeY)
            {
                hackForMouseY = Y;
                MouseMovedEvent(new Int2(X, Y), new Int2(RelativeX, Y - hackForMouseY));
            }

            void OnMouseWheel()
            {
                    
            }

            public void SetCursorMode(MouseState state)
            {
                SDL.SDL_ShowCursor(state.MouseVisible ? 1 : 0);
                SDL.SDL_SetRelativeMouseMode(state.MouseTrapped ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
            }
            
            public MouseState GetMouseMode()
            {
                return CurrentMouseState;
            }


            void OnClose()
            {
                _gameInstance.GameEnded();
                Engine.Renderer.Dispose();
                Console.WriteLine("Closed!");
                closed = true;
            }

            public Action<Keycode, KeyModifiers,char, bool> KeyPressedEvent = (_, _, _, _) =>
            {
            };
            
            public Action<Keycode> KeyReleasedEvent = _ =>
            {
            };
            
            public Action<Int2, Int2> MouseMovedEvent =  (_, _) =>
            {
            };
    }
}