using System;
using System.Diagnostics;
using System.Numerics;
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

    /// <summary>
    /// Rendering Logic goes here, potentially could be the base for other rendering backends.
    /// </summary>
    public class Renderer
    {
        object thing = new object();
        public UniformBuffer<Matrix4x4> ViewProjBuffer;
        public UniformBuffer<Matrix4x4> WorldBuffer;
        
        ImGuiRenderer _imGuiHandler;

        Renderpass testPass;
        
        internal Renderer(IView viewport)
        {
            Device = viewport.CreateGraphicsDevice(new GraphicsDeviceOptions(false, PixelFormat.R32_Float,false, ResourceBindingModel.Default, true, true),GraphicsBackend.Direct3D11);
            _list = Device.ResourceFactory.CreateCommandList();
            ViewProjBuffer = new UniformBuffer<Matrix4x4>(Device, 2);
            WorldBuffer = new UniformBuffer<Matrix4x4>(Device, 1);
            _imGuiHandler = new ImGuiRenderer(Device, Device.SwapchainFramebuffer.OutputDescription, viewport, InputHandler.Context);
            
            testPass = new BasicMaterialRenderPass(Camera.MainCamera, this);

            //Device.SyncToVerticalBlank = true;
        }

        public GraphicsDevice Device;
        CommandList _list; 
        Stopwatch _stopwatch = new Stopwatch();
        public uint FPS;
        Plane[] sides = new Plane[6];
        internal void OnRender(double time)
        {
            if (Camera.MainCamera == null)
                return;
            
            
            Span<Matrix4x4> UpdateMatrix = stackalloc Matrix4x4[2];
            
            _stopwatch.Restart();
            
            
            UpdateMatrix[0] = Camera.MainCamera.GetProjectionMatrix();
            UpdateMatrix[1] = Camera.MainCamera.GetViewMatrix();
            ViewProjBuffer.ModifyBuffer(UpdateMatrix, Device);

            if (_list == null)
            {
                _list = Device.ResourceFactory.CreateCommandList();
            }
            
            _list.Begin();
            _list.SetFramebuffer(Device.SwapchainFramebuffer);
            _list.ClearColorTarget(0, RgbaFloat.Black);
            _list.ClearDepthStencil(1f);

            
            
            
            testPass.RunPass(_list);
            
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
                FPS = (uint) (1/_stopwatch.Elapsed.TotalSeconds);   
            }

        }
        
        internal void OnResize(Vector2D<int> size)
        {
            // Adjust the viewport to the new window size
            Device.ResizeMainWindow((uint)size.X,(uint)size.Y);
        }
    }
}