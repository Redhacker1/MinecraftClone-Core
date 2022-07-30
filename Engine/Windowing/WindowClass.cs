using System;
using Engine.Input;
using Engine.Objects;
using Engine.Rendering.Veldrid;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Windowing
{
    public class WindowClass
    {
        public Shader Shader;
        public Texture Texture;

        public static Renderer Renderer { get; protected set; }

        readonly GameEntry _gameInstance;
        public static IWindow Handle;

        public WindowClass(WindowOptions options, GameEntry GameClass)
        {
            Handle = Window.Create(options);
            Handle.IsContextControlDisabled = true;


            //Assign events.
            Handle.Update += Update;
            Handle.Load += OnLoad;
            Handle.Closing += OnClose;

            _gameInstance = GameClass;
        }
        
        public WindowClass(IWindow windowHandle, GameEntry gameClass)
        {
            Handle = windowHandle;

            //Assign events.
            //Handle.Update += Update;
            Handle.Load += OnLoad;
            Handle.Closing += OnClose;

            _gameInstance = gameClass;
        }
        

        void OnLoad()
        {
            IInputContext context = Handle.CreateInput();
            InputHandler.InitInputHandler(context);

            Renderer = new Renderer(Handle);
            Handle.FramebufferResize += Renderer.OnResize;
            Handle.Render += Renderer.OnRender;

            _gameInstance.Gamestart();
            
        }

        void OnClose()
        {
            _gameInstance.GameEnded();
            Renderer.Dispose();
        }
        


        double physicsDelta;
        void Update(double delta)
        {

            InputHandler.PollInputs();
            physicsDelta += delta;

            bool physicsProcess = physicsDelta >= 0.0166666;

            for (int  index = 0;  index < GameObject.Objects.Count; index++)
            {
                WeakReference<GameObject> objectReference = GameObject.Objects[index];
                GameObject gameObject = null;
                bool successful = false;
                if (objectReference != null)
                {
                    successful = objectReference.TryGetTarget(out gameObject);
                }

                
                if (successful)
                {

                    if (gameObject.cleanup)
                    {
                        gameObject.Dispose();
                        continue;
                    }

                    if (gameObject.Started != true)
                    {
                        gameObject._Ready();
                        gameObject.Started = true;
                    }
                    if (gameObject.Ticks)
                    {
                        gameObject._Process(delta);
                    }
                    if (physicsProcess && gameObject.PhysicsTick)
                    {
                        // TODO: Implement physics sub-stepping when on low frame rates
                        gameObject._PhysicsProcess(physicsDelta);
                    }
                }
                else
                {
                    GameObject.Objects.Remove(objectReference);
                }
            }

            if (physicsProcess)
            {
                physicsDelta = 0;
            }
        }
    }
}