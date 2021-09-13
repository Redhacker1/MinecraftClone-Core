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
        public IWindow handle;
        static IKeyboard KeyboardHandle;
        
        //Used to track change in mouse movement to allow for moving of the Camera
        private static Vector2 LastMousePosition;
        
        DebugProc messageHandler = ((source, type, id, severity, length, message, param) =>
        {
            if (severity ==GLEnum.DebugSeverityHigh)
            {
                Console.WriteLine(Marshal.PtrToStringAnsi(message,  length + 1));
                // Useful if you want to pause after message is printed.
                int a = 0;
                //Only Really useful if we cannot debug the application, otherwise, not worth using
                //TODO: Move Logging and console system over to engine, so we can log problems like this! (Console requires UI to be implemented and preferably an actual input system put in place)
                #if !DEBUG 
                //Console.WriteLine(Environment.StackTrace);
                #endif
            }
            //Environment.Exit(1);
        });
        
        public WindowClass(Glfw GLFWHandle, IWindow windowHandle, Game GameClass)
        {
            GlfwHandle = GLFWHandle;
            handle = windowHandle;

            //Assign events.
            handle.Update += Update;
            handle.Render += OnRender;
            handle.Load += OnLoad;
            handle.Closing += OnClose;

            gameinstance = GameClass;
        }

        void OnLoad()
        {
            GlHandle = GL.GetApi(handle);
            
            Shader = new Shader(GlHandle, @"Assets\shader.vert", @"Assets/shader.frag");

            Texture = new Texture(GlHandle, @"Assets\TextureAtlas.tga");

            IInputContext context = handle.CreateInput();
            InputHandler.InitInputHandler(context);
            
            
            gameinstance.Gamestart();
        }

        void OnClose()
        {
            gameinstance.GameEnded();
        }

        void OnRender(double time)
        {
            Mesh mesh = null;
            //Console.WriteLine("Running");

            // = time;
            GlHandle.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            GlHandle.Enable(EnableCap.DepthTest);
            GlHandle.Enable(EnableCap.CullFace);
            GlHandle.Enable(EnableCap.DebugOutput);
            GlHandle.DepthFunc(DepthFunction.Lequal);
            GlHandle.Enable(GLEnum.DebugOutputSynchronous);
            GlHandle.DebugMessageCallback(messageHandler, new ReadOnlySpan<int>());



            //Console.WriteLine($"{Meshes.Count}, meshes compared to {Mesh.Meshes.Count} Total");
            if (Camera.MainCamera != null)
            {
                Shader?.SetUniform("uView", Camera.MainCamera.GetViewMatrix());
                Shader?.SetUniform("uProjection", Camera.MainCamera.GetProjectionMatrix());
            }

            for (int i = 0; i < Mesh.OutofDateMeshes.Count; i++)
            {

                mesh = Mesh.OutofDateMeshes[i];

                if (mesh != null)
                {
                    mesh.RegenerateVao();
                    Mesh.OutofDateMeshes.Remove(mesh);
                }
            }
            
            // FIXME: This system is not thread safe!
            for (int i = 0; i < Mesh.QueuedForRemoval.Count; i++)
            {
                Mesh meshdata = Mesh.QueuedForRemoval[i];
                if (meshdata != null)
                {
                    meshdata?.Dispose();
                    Mesh.Meshes.Remove(meshdata);
                }

                Mesh.QueuedForRemoval.Remove(meshdata);
            }
            
            //Console.WriteLine("Rendering models");
            //Console.WriteLine($"{Mesh.Meshes.Count} meshes to draw");
            for (int meshindex = 0; meshindex < Mesh.Meshes.Count; meshindex++)
            {


                GlHandle.CullFace(CullFaceMode.Front);
                mesh = Mesh.Meshes[meshindex];
                Texture?.Bind();
                Shader?.Use();
                
                
                if (mesh?.Deleted == false && mesh?.MeshReference != null)
                {
                    mesh.MeshReference.Bind();
                    Shader?.SetUniform("uModel", mesh.ViewMatrix);
                    Shader?.SetUniform("uTexture0", 0);
                    GlHandle.DrawArrays(GLEnum.Triangles, 0, mesh.MeshReference.Vertexcount);
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
                    //ConsoleLibrary.DebugPrint("TODO: FIXME");
                }
            }

            if (physicsProcess)
            {
                physicsDelta = 0;
            }
        }
    }
}