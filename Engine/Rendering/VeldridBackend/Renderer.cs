using System;
using System.Diagnostics;
using Engine.Input;
using Engine.Renderable;
using Engine.Rendering.Abstract.View;
using Engine.Utilities.MathLib;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using Veldrid;

// TODO: This should be seperated into implementation and Logic files, Veldrid should NOT be a core dependency.
// TODO: Look into making user definable or events to trigger on user defined times, say when a frame is completed or a new frame is starting.
// TODO: The Level should probably have control over rendering the game, either that or the Game Entry, I am inclined to give it to the level class
namespace Engine.Rendering.VeldridBackend
{

    /// <summary>
    /// Rendering Logic goes here, potentially could be the base for other rendering backends.
    /// </summary>
    public class Renderer : IDisposable
    {


        readonly ImGuiRenderer _imGuiHandler;
        
        
        internal Renderer(IView viewport)
        {
            Device = viewport.CreateGraphicsDevice(new GraphicsDeviceOptions(false, PixelFormat.R32_Float, false, ResourceBindingModel.Improved, true, true), GraphicsBackend.Direct3D11);

            _imGuiHandler = new ImGuiRenderer(Device, Device.SwapchainFramebuffer.OutputDescription, viewport, InputHandler.Context);
            
            _stopwatch.Start();
        }
        
        Stopwatch _stopwatch = new Stopwatch();
        public float FPS;
        public GraphicsDevice Device;

        Vector2D<int> newSize = new Vector2D<int>();

        public void RenderImgGui(double time, CommandList list)
        {
            _imGuiHandler.WindowResized(new Vector2D<int>((int) Device.SwapchainFramebuffer.Width, (int) Device.SwapchainFramebuffer.Height));
            _imGuiHandler.Update((float) time);
            for (int index = ImGUIPanel.Panels.Count - 1; index >= 0; index--)
            {
                ImGUIPanel uiPanel = ImGUIPanel.Panels[index];
                if (uiPanel.Draggable == false)
                {
                    ImGui.SetNextWindowPos(uiPanel.Position, ImGuiCond.Once);
                }

                ImGui.Begin(uiPanel.PanelName, uiPanel.Flags);
                uiPanel.CreateUI();
                uiPanel.Position = ImGui.GetWindowPos();
                ImGui.End();
            }

            _imGuiHandler.Render(Device, list);

            if (_stopwatch.ElapsedMilliseconds != 0)
            {
                FPS = (float)_stopwatch.Elapsed.TotalMilliseconds;
            }
            _stopwatch.Restart();

        }
        


        void ResizeMainSwapchain(Vector2D<int> size)
        {
            if (size.Length > 0)
            {
                // Adjust the viewport to the new window size
                Engine.MainFrameBuffer.Size = new Int2(size.X, size.Y);

            }
        }


        internal void RunCommandList(CommandList list)
        {
            Device.SubmitCommands(list);
        }
        
        internal void SwapBuffers()
        {
            Device.SwapBuffers();
        }


        internal void Resize(Vector2D<int> size)
        {
            Console.WriteLine("resized!");
            ResizeMainSwapchain(size);
        }

        bool _disposing;
        

        public void Dispose()
        {
            if (_disposing)
            {
                return;
            }
            Console.WriteLine("Telling render system to stop and dispose of objects");
            // Stop rendering frames and let the GPU flush it's current work, then dispose of the GraphicsDevice
            _disposing = true;
            Device.WaitForIdle();
            GC.SuppressFinalize(this);
            // Dispose of RenderTargets
            foreach (RenderTarget renderTarget in RenderTarget.Targets)
            {
                if (renderTarget.ValidTarget)
                {
                    renderTarget.Dispose();
                }
            }
            Device.DisposeWhenIdle(Device);
        }
    }
}