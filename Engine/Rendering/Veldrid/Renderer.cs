﻿using System;
using System.Diagnostics;
using Engine.Input;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using Veldrid;

// TODO: This should be seperated into implementation and Logic files, Veldrid should NOT be a core dependency.
// TODO: Look into making user definable or events to trigger on user defined times, say when a frame is completed or a new frame is starting.
namespace Engine.Rendering.Veldrid
{

    /// <summary>
    /// Rendering Logic goes here, potentially could be the base for other rendering backends.
    /// </summary>
    public class Renderer : IDisposable
    {
        public RenderPass[] Passes = new RenderPass[16];

        public bool AddPass(int index, RenderPass pass)
        {
            lock (Passes)
            {
                if (index > 0 && Passes.Length > index)
                {
                    Passes[index] = pass;
                    return true;
                }   
            }
            return false;
        }

        public bool RemovePass(int index)
        {
            lock (Passes)
            {
                if (index < Passes.Length)
                {
  
                    Passes[index] = null;
                }
            }
            return false;
        }


        

        readonly ImGuiRenderer _imGuiHandler;
        
        
        internal Renderer(IView viewport)
        {
            Device = viewport.CreateGraphicsDevice(new GraphicsDeviceOptions(false, PixelFormat.R32_Float, false, ResourceBindingModel.Improved, true, true), GraphicsBackend.Vulkan);
            _list = Device.ResourceFactory.CreateCommandList();
            _imGuiHandler = new ImGuiRenderer(Device, Device.SwapchainFramebuffer.OutputDescription, viewport, InputHandler.Context);

            // TODO: probably should implement some better systems for dealing with this shit!
            //Passes[0] = new DepthPrepass(this);
            Passes[0] = new DefaultRenderPass(this);

        }
        
        CommandList _list; 
        Stopwatch _stopwatch = new Stopwatch();
        public uint FPS;
        public GraphicsDevice Device;
        internal void OnRender(double time)
        {
            if (_disposing)
            {
                return;
            }
            _stopwatch.Restart();
            _list.Begin();
            _list.SetFramebuffer(Device.SwapchainFramebuffer);
            _list.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            _list.ClearDepthStencil(1f);
            
            lock (Passes)
            {
                foreach (RenderPass pass in Passes)
                {
                    pass?.RunPass(_list);
                }
            }

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
                FPS = (uint) (1 / time);   
            }

        }
        
        internal void OnResize(Vector2D<int> size)
        {
            // Adjust the viewport to the new window size
            Device.ResizeMainWindow((uint)size.X,(uint)size.Y);
        }

        bool _disposing;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (_disposing)
            {
                return;
            }
            
            // Stop rendering frames and let the GPU flush it's current work, then dispose of the GraphicsDevice
            _disposing = true;
            Device.WaitForIdle();
            Device.Dispose();
        }
    }
}