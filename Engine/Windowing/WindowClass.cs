using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Engine.Input;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Texture = Engine.Rendering.Texture;
using Shader = Engine.Rendering.Shader;

namespace Engine.Windowing
{
    public unsafe class WindowClass
    {
        public Shader Shader;
        public Texture Texture;


        Game gameinstance;
        Glfw GlfwHandle;
        static public GL GlHandle;
        internal static IWindow Handle;
        static IKeyboard KeyboardHandle;
        
        //Used to track change in mouse movement to allow for moving of the Camera
        private static Vector2 LastMousePosition;
        
        DebugProc messageHandler = ((source, type, id, severity, length, message, param) =>
        {
            Console.WriteLine(Marshal.PtrToStringAnsi(message,  length + 1));
            // Useful if you want to pause after message is printed.
            int a = 0;
            //Environment.Exit(1);
        });
        
        public WindowClass(Glfw GLFWHandle, IWindow windowHandle, Game GameClass)
        {
            GlfwHandle = GLFWHandle;
            Handle = windowHandle;

            //Assign events.
            Handle.Update += Update;
            Handle.Render += OnRender;
            Handle.Load += OnLoad;
            Handle.Closing += OnClose;

            gameinstance = GameClass;
        }

        void OnLoad()
        {
            GlHandle = GL.GetApi(Handle);
            //GlHandle.DebugMessageCallback(messageHandler, new ReadOnlySpan<int>());
            
            Shader = new Shader(GlHandle, @"Assets\shader.vert", @"Assets/shader.frag");

            Texture = new Texture(GlHandle, @"Assets\TextureAtlas.tga");

            IInputContext context = Handle.CreateInput();
            InputHandler.InitInputHandler(context);
            
            
            gameinstance.Gamestart();
        }

        void OnClose()
        {
            gameinstance.GameEnded();
        }

        void OnRender(double time)
        {
            //Console.WriteLine("Running");

            // = time;
            GlHandle.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            GlHandle.Enable(EnableCap.DepthTest);
            GlHandle.Enable(EnableCap.CullFace);
            GlHandle.Enable(EnableCap.DebugOutput);
            GlHandle.DepthFunc(DepthFunction.Lequal);
            //GlHandle.Enable(GLEnum.DebugOutputSynchronous);



            //Console.WriteLine($"{Meshes.Count}, meshes compared to {Mesh.Meshes.Count} Total");
            if (Camera.MainCamera != null)
            {
                Shader?.SetUniform("uView", Camera.MainCamera.GetViewMatrix());
                Shader?.SetUniform("uProjection", Camera.MainCamera.GetProjectionMatrix());
            }

            for (int meshindex = 0; meshindex < Mesh.Meshes.Count; meshindex++)
            {
                var mesh = Mesh.Meshes[meshindex];
                if (mesh != null)
                {
                    if (mesh.ActiveState == MeshState.Delete)
                    {
                        mesh.Dispose();
                        Mesh.Meshes.Remove(mesh);
                    }
                    else if (mesh.ActiveState == MeshState.Dirty)
                    {
                        mesh.MeshReference?.Dispose();
                        mesh.MeshReference = mesh.RegenerateVao();
                        mesh.ActiveState = MeshState.Normal;
                    }
                    if(mesh.ActiveState == MeshState.Normal)
                    {
                        GlHandle.CullFace(CullFaceMode.Front);
                        Texture?.Bind();
                        Shader?.Use();
                
                
                        if (mesh?.MeshReference != null && mesh.ActiveState == MeshState.Normal)
                        {
                            mesh.MeshReference.Bind();
                            Shader?.SetUniform("uModel", mesh.ViewMatrix);
                            Shader?.SetUniform("uTexture0", 0);
                            GlHandle.DrawArrays(GLEnum.Triangles, 0, mesh.MeshReference.Vertexcount);
                        }    
                    }
                    
                }
            }

            //TODO: Layered UI/PostProcess
            

        }
        

        double physicsDelta = 0;
        void Update(double delta)
        {

            InputHandler.PollInputs();
            physicsDelta += delta;

            bool physicsProcess = physicsDelta >= 0.0166666;

            for (int i = 0; i < GameObject.Objects.Count; i++)
            {
                try 
                {
                    GameObject @object = GameObject.Objects[i];
                        
                    @object?._Process((float) delta);
                    if (physicsProcess)
                    {
                        @object?._PhysicsProcess((float) physicsDelta);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    
                    // Maybe use a deferred deletion scheme?
                }
            }

            if (physicsProcess)
            {
                physicsDelta = 0;
            }
        }
    }
}