using System.Diagnostics;
using Engine.Input;
using Engine.Objects;
using Engine.Rendering;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Shader = Engine.Rendering.Shader;
using Texture = Engine.Rendering.Texture;

namespace Engine.Windowing
{
    public class WindowClass
    {
        public Shader Shader;
        public Texture Texture;

        public static Renderer _renderer;
        
        Game gameinstance;
        internal static IWindow Handle;

        public WindowClass(int width, int height, int posx, int posy, string WindowName, Game GameClass)
        {

            WindowOptions options = WindowOptions.Default;
            options.Size = new Vector2D<int>(width, height);
            options.Position = new Vector2D<int>(posx, posy);
            options.Title = WindowName;
            options.VSync = false;
            
            Handle = Window.Create(options);
            Handle.IsContextControlDisabled = true;


            //Assign events.
            Handle.Update += Update;
            //Handle.Render += OnRender;
            Handle.Load += OnLoad;
            Handle.Closing += OnClose;

            gameinstance = GameClass;
        }
        
        public WindowClass(IWindow windowHandle, Game GameClass)
        {
            Handle = windowHandle;

            //Assign events.
            Handle.Update += Update;
            Handle.Load += OnLoad;
            Handle.Closing += OnClose;

            gameinstance = GameClass;
        }
        
        float framesPerSecond = 0.0f;
        static int fps;
        float lastTime = 0.0f;
        double currentTime;
        int CalculateFPS(double time)
        {
            currentTime += time;
            if (currentTime >= 1f)
            {
                fps = 0;
                currentTime = 0;
            }

            fps++;
            
            return fps;
        }
        

        void OnLoad()
        {
            IInputContext context = Handle.CreateInput();
            InputHandler.InitInputHandler(context);

            _renderer = new Renderer(Handle);
            Handle.FramebufferResize += _renderer.OnResize;
            Handle.Render += _renderer.OnRender;
            
            gameinstance.Gamestart();
            
        }

        void OnClose()
        {
            gameinstance.GameEnded();
        }
        
        Stopwatch timer = Stopwatch.StartNew();


        double physicsDelta;
        void Update(double delta)
        {

            InputHandler.PollInputs();
            physicsDelta += delta;

            bool physicsProcess = physicsDelta >= 0.0166666;

            for (int  index = 0;  index < GameObject.Objects.Count; index++)
            {
                var gameObject = GameObject.Objects[index];
                if (gameObject != null)
                {
                    if (gameObject.Started != true)
                    {
                        gameObject._Ready();
                        gameObject.Started = true;
                    }
                    
                    gameObject._Process(delta);
                    if (physicsProcess && gameObject.PhysicsTick)
                    {
                        gameObject._PhysicsProcess(physicsDelta);
                    }
                }
            }

            if (physicsProcess)
            {
                physicsDelta = 0;
            }
        }
    }
}