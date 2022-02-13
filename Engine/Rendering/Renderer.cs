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

namespace Engine.Rendering
{
    internal struct ViewProj
    {
        public Matrix4x4 view;
        public Matrix4x4 Projection;

    }

    /// <summary>
    /// Rendering Logic goes here, potentially could be the base for other rendering backends.
    /// </summary>
    public class Renderer
    {
        object thing = new object();
        
        public UniformBuffer<Matrix4x4> ProjectionBuffer;
        public UniformBuffer<Matrix4x4> ViewBuffer;
        public UniformBuffer<Matrix4x4> WorldBuffer;
        
        ImGuiRenderer _imGuiHandler;
        ViewProj _worldDataBuffer;
        internal unsafe Renderer(IView viewport)
        {
            Device = viewport.CreateGraphicsDevice(new GraphicsDeviceOptions(false, PixelFormat.R16_UNorm,false, ResourceBindingModel.Default, true, true),GraphicsBackend.Vulkan);
            _list = Device.ResourceFactory.CreateCommandList();
            ProjectionBuffer = new UniformBuffer<Matrix4x4>(Device, 1);
            ViewBuffer = new UniformBuffer<Matrix4x4>(Device, 1);
            WorldBuffer = new UniformBuffer<Matrix4x4>(Device, 1);
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
            Span<Matrix4x4> UpdateMatrix = stackalloc Matrix4x4[1];
            
            _stopwatch.Restart();
            //_stopwatch.Start();
            if (Camera.MainCamera != null)
            {
                UpdateMatrix[0] = Camera.MainCamera.GetProjectionMatrix();
                ProjectionBuffer.ModifyBuffer(UpdateMatrix, Device);
                UpdateMatrix[0] = Camera.MainCamera.GetViewMatrix();
                ViewBuffer.ModifyBuffer(UpdateMatrix, Device);
            }
            frustum =  Camera.MainCamera?.GetViewFrustum(sides);
            
            _list.Begin();
            _list.SetFramebuffer(Device.SwapchainFramebuffer);
            _list.ClearColorTarget(0, RgbaFloat.Black);
            _list.ClearDepthStencil(1f);

            
            
            
            foreach (var materials in Material._materials)
            {
                foreach (var instance in materials._instances)
                {
                    
                }
            }

            Mesh[] sceneMeshes = Mesh.Meshes.ToArray();
            List<Mesh> meshes = new List<Mesh>(Mesh.Meshes.Count);
            
            
            
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
                    UpdateMatrix[0] = mesh.ViewMatrix;
                    WorldBuffer.ModifyBuffer(UpdateMatrix, _list, Device);

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
    }
}