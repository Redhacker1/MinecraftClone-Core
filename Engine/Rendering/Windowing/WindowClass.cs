using System;
using System.Threading;
using Engine.Input;
using Engine.Renderable;
using Engine.Rendering.Shared;
using Engine.Rendering.VeldridBackend;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Rendering.Windowing
{
    public sealed class WindowClass
    {
        public static VeldridRenderer Backend;
        Game _gameinstance;
        internal IWindow Handle;
        public WindowClass(IWindow windowHandle, Game game, VeldridRenderer renderer)
        {
            Backend = renderer;
            Handle = windowHandle;
            _gameinstance = game;

            //Assign events.
            //Handle.Load += OnLoad;
            Handle.Load +=  () =>
            {
                Console.WriteLine(Environment.CurrentManagedThreadId);
                renderer.OnLoad();
                OnLoad();
            };
            
            
            Handle.Closing += OnClose;
            Handle.Update += PollInputs;
        }

        void OnLoad()
        {
            Handle.IsContextControlDisabled = true;
            IInputContext context = Handle.CreateInput();
            InputHandler.InitInputHandler(context);

            // Initialize remaining callbacks
            Handle.Render += (delta) =>
            {
                Backend.OnRender(delta, MeshInstance.MeshInstances);
            };
            Handle.Resize += Backend.OnResized;
            Handle.Update += _gameinstance.Update;
            
            // Bootstrap game logic
            _gameinstance.Gamestart();
        }

        void OnClose()
        {
            _gameinstance.GameEnded();
        }
        
        static void PollInputs(double notUsed)
        {
            InputHandler.PollInputs();
        }
    }
}