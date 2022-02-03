using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Input;
using Engine.Renderable;
using Engine.Rendering.Culling;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using Veldrid;
using Plane = Engine.Rendering.Culling.Plane;


internal struct ViewProj
{
    public Matrix4x4 view;
    public Matrix4x4 Projection;

}
namespace Engine.Rendering
{
    
    /// <summary>
    /// Rendering Logic goes here, potentially could be the base for other rendering backends.
    /// </summary>
    public class Renderer
    {
        object thing = new object();
        public DeviceBuffer ProjectionBuffer;
        public DeviceBuffer ViewBuffer;

        public DeviceBuffer WorldBuffer;
        ImGuiRenderer _imGuiHandler;
        ViewProj _worldDataBuffer;
        internal Renderer(IView viewport)
        {
            Device = viewport.CreateGraphicsDevice(new GraphicsDeviceOptions(false, PixelFormat.R16_UNorm,false, ResourceBindingModel.Default, true, true),GraphicsBackend.Direct3D11);
            _list = Device.ResourceFactory.CreateCommandList();
            ProjectionBuffer = Device.ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ViewBuffer = Device.ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            WorldBuffer = Device.ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _imGuiHandler = new ImGuiRenderer(Device, Device.SwapchainFramebuffer.OutputDescription, viewport,
                InputHandler.Context);

            //Device.SyncToVerticalBlank = true;
        }

        public GraphicsDevice Device;
        readonly CommandList _list; 
        Stopwatch _stopwatch = new Stopwatch();
        public uint FPS = 0;
        Frustrum? frustum;
        Plane[] sides = new Plane[6];
        internal void OnRender(double time)
        {

            _stopwatch.Restart();
            //_stopwatch.Start();
            if (Camera.MainCamera != null)
            {
                Device.UpdateBuffer(ProjectionBuffer, 0 , Camera.MainCamera.GetProjectionMatrix());
                Device.UpdateBuffer(ViewBuffer, 0 , Camera.MainCamera.GetViewMatrix());
            }
            frustum =  Camera.MainCamera?.GetViewFrustum(sides);
            
            _list.Begin();
            _list.SetFramebuffer(Device.SwapchainFramebuffer);
            _list.ClearColorTarget(0, RgbaFloat.Black);
            _list.ClearDepthStencil(1f);


            Mesh[] sceneMeshes = Mesh.Meshes.ToArray();
            List<Mesh> meshes = new List<Mesh>(Mesh.Meshes.Count /2);
            
            
            
            Parallel.ForEach(sceneMeshes, (mesh) =>
            {
                if (IntersectionHandler.MeshInFrustrum(mesh, frustum))
                {
                    lock (thing)
                    {
                        meshes.Add(mesh);
                    }
                }
            });

            Material prevmaterial = null;
            foreach (Mesh mesh in meshes)
            {
                if(!mesh.UpdatingMesh && mesh.BindResources(_list, prevmaterial))
                {
                    _list.UpdateBuffer(WorldBuffer, 0, mesh.ViewMatrix);

                    if (mesh.UseIndexedDrawing)
                    {
                        _list.DrawIndexed(mesh.VertexElements);
                    }
                    else
                    {
                        _list.Draw(mesh.VertexElements);   
                    }

                    prevmaterial = mesh.MeshMaterial;
                }

            }

            //Console.WriteLine($"Drawing {ImGUIPanel.panels.Count} UI Elements!");
            _imGuiHandler.Update((float) time);
            foreach (ImGUIPanel uiPanel in ImGUIPanel.panels)
            {

                if (uiPanel.Draggable == false)
                {
                    ImGui.SetNextWindowPos(uiPanel.Position, ImGuiCond.Once);   
                }
                ImGui.Begin(uiPanel.PanelName, uiPanel.Flags);
                uiPanel.CreateUI();
                uiPanel.Position = ImGui.GetWindowPos();
                ImGui.End();
            }
            _imGuiHandler.Render(Device, _list);
            
            
            _list.End();
            Device.SubmitCommands(_list);
            Device.SwapBuffers();


            if (_stopwatch.ElapsedMilliseconds != 0)
            {
                FPS = (uint) (1000f/ _stopwatch.Elapsed.TotalMilliseconds);   
            }
            //Console.WriteLine(FPS);


            //Device.WaitForIdle();

        }
        
        internal void OnResize(Vector2D<int> size)
        {
            // Adjust the viewport to the new window size
            Device.ResizeMainWindow((uint)size.X,(uint)size.Y);
        }
        /*
        void OnRender(double time)
        {
            Frustrum? frustum =  Camera.MainCamera?.GetViewFrustum();

            GlHandle.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            GlHandle.Enable(EnableCap.DepthTest);
            GlHandle.Enable(EnableCap.CullFace);
            GlHandle.Enable(EnableCap.DebugOutput);
            GlHandle.DepthFunc(DepthFunction.Lequal);


            
            if (Camera.MainCamera != null)
            {
                Shader?.SetUniform("uView", Camera.MainCamera.GetViewMatrix());
                Shader?.SetUniform("uProjection", Camera.MainCamera.GetProjectionMatrix());
            }

            int MeshesDrawn = 0;
            Mesh mesh;
            for (int meshindex = 0; meshindex < Mesh.Meshes.Count; meshindex++)
            {
                mesh = Mesh.Meshes[meshindex];
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
                        mesh.ActiveState = MeshState.Render;
                    }
                    if(mesh.ActiveState == MeshState.Render)
                    {


                        if (mesh?.MeshReference != null && mesh.ActiveState == MeshState.Render && IntersectionHandler.MeshInFrustrum(mesh, ref frustum))
                        {
                            
                            GlHandle.CullFace(CullFaceMode.Front);
                            Texture?.Bind();
                            Shader?.Use();
                            
                            mesh.BindBuffers();
                            Shader?.SetUniform("uModel", mesh.ViewMatrix);
                            //Shader?.SetUniform("uTexture0", 0);
                            mesh.Draw(c);
                        }
                        else
                        {
                            MeshesDrawn += 1;
                        }
                    }
                    
                }
            }

            //TODO: Layered UI/PostProcess

            // TODO: Re-enable IMGUI code when everything is working properly.
            /*
            foreach (ImGUIPanel uiPanel in ImGUIPanel.panels)
            {
                ImGui.Begin(uiPanel.PanelName);
                uiPanel.CreateUI();
                ImGui.End();
            }
            GuiController.Render();
            
        }
        */
    }
}