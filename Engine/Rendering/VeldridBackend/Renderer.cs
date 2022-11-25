using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Engine.Input;
using Engine.Renderable;
using Engine.Rendering.Abstract.RenderStage;
using Engine.Rendering.Abstract.View;
using Engine.Utilities.MathLib;
using Engine.Windowing;
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
        public readonly Thread RenderThread;



        readonly ImGuiRenderer _imGuiHandler;
        
        
        internal Renderer(IView viewport)
        {
            Device = viewport.CreateGraphicsDevice(new GraphicsDeviceOptions(
                false, PixelFormat.D32_Float_S8_UInt, false, ResourceBindingModel.Improved, true, true), GraphicsBackend.Direct3D11);
            _list = Device.ResourceFactory.CreateCommandList();
            
            _imGuiHandler = new ImGuiRenderer(Device, Device.SwapchainFramebuffer.OutputDescription, viewport, InputHandler.Context);
            RenderThread = new Thread(() =>
            {
                while (!_disposing)
                {
                    if (Pause)
                    {
                        Device.WaitForIdle();
                        continue;
                    }
                    
                    OnRender(_stopwatch.Elapsed.TotalSeconds);
                    
                    if (_stopwatch.ElapsedMilliseconds != 0)
                    {
                        FPS = (uint) (1f / _stopwatch.Elapsed.TotalSeconds);   
                    }
                    _stopwatch.Restart();

                    RendererHalted = Pause;
                }

                RendererHalted = true;
            });
            OnResize = _ => { Resize(WindowClass.Handle.Size);};
        }
        
        CommandList _list;
        Stopwatch _stopwatch = new Stopwatch();
        public uint FPS;
        public GraphicsDevice Device;
        public bool Pause = false;
        public bool RendererHalted = true;

        Vector2D<int> newSize = new Vector2D<int>();

        internal void OnRender(double time)
        {
            if (SizeChange)
            {
                ResizeMainSwapchain(newSize);
            }
            
            // Clear the screen, 
            _list.Begin();
            _list.PushDebugGroup("Clear Screen");
            _list.SetFramebuffer(Device.SwapchainFramebuffer);
            _list.ClearColorTarget(0, new RgbaFloat(.29804f,.29804f, .32157f, 1f));
            _list.ClearDepthStencil(1, 0);
            _list.PopDebugGroup();
            _list.End();
            Device.SubmitCommands(_list);
            
            
            _list.Begin();
            _list.SetFramebuffer(Device.SwapchainFramebuffer);


            ImmutableArray<RenderTarget> renderTargets = RenderTarget.Targets.ToImmutableArray();
            foreach (RenderTarget target in renderTargets)
            {
                if (target.ValidTarget)
                {
                    target.Flush(
                        new RenderState()
                        {
                            Device = Device,
                            GlobalCommandList = _list
                        }
                    );
                }
            }
            

            _imGuiHandler.Update((float) time);
            for (int index = 0; index < ImGUIPanel.Panels.Count; index++)
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

            _imGuiHandler.Render(Device, _list);
            _list.End();
            
            Device.SubmitCommands(_list);
            Device.SwapBuffers();
            
            if (_stopwatch.ElapsedMilliseconds != 0)
            {
                FPS = (uint) (1 / _stopwatch.Elapsed.TotalMilliseconds);   
            }

        }

        bool SizeChange = false;


        void ResizeMainSwapchain(Vector2D<int> size)
        {
            if (size.Length > 0)
            {
                // Adjust the viewport to the new window size

                Engine.MainFrameBuffer.Size = new Int2(size.X, size.Y);
                _imGuiHandler.WindowResized(size);
                
            }
        }

        public readonly Action<Vector2D<int>> OnResize;


        internal void Resize(Vector2D<int> size)
        {
            SizeChange = true;
            newSize = size;
        }

        bool _disposing;


        void WaitForHalt()
        {
            Device.WaitForIdle();
            while (RendererHalted == false)
            {
                
            }
        }

        public void Dispose()
        {
            
            Pause = true;
            if (_disposing)
            {
                return;
            }
            Console.WriteLine("Telling render system to stop and dispose of objects");
            // Stop rendering frames and let the GPU flush it's current work, then dispose of the GraphicsDevice
            _disposing = true;
            WaitForHalt();
            RenderThread.Join();

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