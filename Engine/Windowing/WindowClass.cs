using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Engine.Input;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using ImGuiNET;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Shader = Engine.Rendering.Shader;
using Texture = Engine.Rendering.Texture;

namespace Engine.Windowing
{
    public class WindowClass
    {
        public Shader Shader;
        public Texture Texture;


        ImGuiController GuiController;

        Game gameinstance;
        Glfw GlfwHandle;
        static public GL GlHandle;
        internal static IWindow Handle;

        DebugProc messageHandler = ((source, type, id, severity, length, message, param) =>
        {
            //Console.WriteLine(Marshal.PtrToStringAnsi(message,  length + 1));
            // Useful if you want to pause after message is printed.
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
            GlHandle.DebugMessageCallback(messageHandler, new ReadOnlySpan<int>());
            
            Shader = new Shader(GlHandle, @"Assets\shader.vert", @"Assets/shader.frag");

            Texture = new Texture(GlHandle, @"Assets\TextureAtlas.tga");

            IInputContext context = Handle.CreateInput();
            InputHandler.InitInputHandler(context);

            GuiController = new ImGuiController(GlHandle, Handle, context);
            
            gameinstance.Gamestart();
            
        }

        void OnClose()
        {
            gameinstance.GameEnded();
        }

        void OnRender(double time)
        {
            var frustum = Camera.MainCamera.GetViewFrustum();

            GlHandle.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            GlHandle.Enable(EnableCap.DepthTest);
            GlHandle.Enable(EnableCap.CullFace);
            GlHandle.Enable(EnableCap.DebugOutput);
            GlHandle.DepthFunc(DepthFunction.Lequal);
            //GlHandle.FrontFace(FrontFaceDirection.CW);

            int Meshrendered = 0;


            if (Camera.MainCamera != null)
            {
                Shader?.SetUniform("uView", Camera.MainCamera.GetViewMatrix());
                Shader?.SetUniform("uProjection", Camera.MainCamera.GetProjectionMatrix());
            }

            Mesh mesh;
            for (int meshindex = 0; meshindex < Mesh.Meshes.Count; meshindex++)
            {
                mesh = Mesh.Meshes[meshindex];
                if (mesh.ActiveState == MeshState.Delete)
                {
                    Mesh.Meshes.Remove(mesh);
                    mesh.Dispose();
                }
                else if (mesh.ActiveState == MeshState.Dirty)
                {
                    mesh.MeshReference?.Dispose();
                    mesh.MeshReference = mesh.RegenerateVao();
                    mesh.ActiveState = MeshState.Render;
                }

                if (mesh.ActiveState == MeshState.Render)
                {
                    GlHandle.CullFace(CullFaceMode.Front);
                    Texture?.Bind();
                    Shader?.Use();

                    if (mesh.ActiveState == MeshState.Render && IntersectionHandler.MeshInFrustrum(mesh, ref frustum))
                    {
                        Shader?.SetUniform("uModel", mesh.ViewMatrix);
                        Shader?.SetUniform("uTexture0", 0);

                        Meshrendered++;
                        mesh.Draw(GlHandle);
                    }
                }
                //Console.WriteLine(Meshrendered);

                //TODO: Layered UI/PostProcess

                // Finish implementing input system to get mouse mode, leaving here for now!
                if (InputHandler.MousePos(0) != Vector2.Zero)
                {
                    GuiController.Update((float)time);   
                }
                foreach (var uiPanel in ImGUIPanel.panels)
                {
                    ImGui.Begin(uiPanel.PanelName);
                    uiPanel.CreateUI();
                    ImGui.End();
                }
                GuiController.Render();
            }
        }


            double physicsDelta;
            void Update(double delta)
            {

                InputHandler.PollInputs();
                physicsDelta += delta;

                bool physicsProcess = physicsDelta >= 0.0166666;

                foreach (GameObject gameObject in GameObject.Objects)
                {
                    if (gameObject != null)
                    {
                        gameObject._Process(delta);
                        if (physicsProcess)
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