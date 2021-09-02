using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Texture = Tutorial.Texture;
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
            InputHandler.KeyboardHandle = context.Keyboards.FirstOrDefault();
            InputHandler.MouseHandle = context.Mice.FirstOrDefault();
            InputHandler.initKeyboardHandler(context);
            
            
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
            GlHandle.DepthFunc(DepthFunction.Lequal);
            
            Mesh[] meshes = Mesh.Meshes.ToArray();
            

            //Console.WriteLine($"{Meshes.Count}, meshes compared to {Mesh.Meshes.Count} Total");
            
            Shader?.SetUniform("uView", Camera.MainCamera.GetViewMatrix());
            Shader?.SetUniform("uProjection", Camera.MainCamera.GetProjectionMatrix());
            for (int i = 0; i < Mesh.OutofDateMeshes.Count; i++)
            {

                var mesh = Mesh.OutofDateMeshes[i];

                if (mesh != null)
                {
                    mesh.RegenerateVao();
                    Mesh.OutofDateMeshes.Remove(mesh);   
                }
            }
            for (int i = 0; i < Mesh.QueuedForRemoval.Count; i++)
            {
                var mesh = Mesh.QueuedForRemoval[i];
                if (mesh != null)
                {
                    mesh?.Dispose();
                    Mesh.QueuedForRemoval.Remove(mesh);
                }
                
                
                
            }
            
            //Console.WriteLine($"{Mesh.Meshes.Count} meshes to draw");
            for (int meshindex = 0; meshindex < meshes.Length; meshindex++)
            {


                GlHandle.CullFace(CullFaceMode.Front);
                Mesh mesh = meshes[meshindex];
                Texture?.Bind();
                Shader?.Use();
                
                if (mesh?.MeshReference != null)
                {
                    mesh.MeshReference.Bind();
                    Shader?.SetUniform("uModel", mesh.ViewMatrix);
                    Shader?.SetUniform("uTexture0", 0);
                    GlHandle.DrawArrays(GLEnum.Triangles, 0, mesh.MeshReference.Vertexcount);
                }


                //mesh.
            }
            
        }
        

        double physicsDelta = 0;
        void Update(double delta)
        {

            InputHandler.PollKeyboard();
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