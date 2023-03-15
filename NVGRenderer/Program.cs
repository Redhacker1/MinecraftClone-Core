using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering.Abstract;
using Engine.Rendering.Abstract.RenderStage;
using Engine.Rendering.Abstract.View;
using Engine.Rendering.VeldridBackend;
using Engine.Windowing;
using NVGRenderer.Rendering;
using Silk.NET.Input;
using SilkyNvg;
using Veldrid;

namespace NVGRenderer
{
    static class EntryPoint
    {
        public static void Main()
        {
            Init.InitEngine(0,0, 1000, 600, "NVG", new NvgRendererDemo());
        }
    }

    class NvgRendererDemo : GameEntry
    {
        Camera cam = new Camera(new Transform(), -Vector3.UnitZ, Vector3.UnitY,1.3333334F, true );
        NvgItem DemoTest;
        NvgItem PerfGraph;
        NvgRenderPass pass;
        protected override void GameStart()
        {
            pass = new NvgRenderPass(Engine.Engine.Renderer);
            Engine.Engine.MainFrameBuffer.AddPass(1, pass);

            DemoTest = new LineBenchmark();
            PerfGraph = new PerfMonitor();
        }
    }

    class GameObject: EngineObject
    {
        public GameObject()
        {
            Ticks = true;
        }
        protected override void _Process(double delta)
        {
            base._Process(delta);
            if (InputHandler.KeyboardJustKeyPressed(0, Key.D))
            {
                Console.WriteLine("Garbage collected!");
                GC.Collect(0, GCCollectionMode.Forced, true, true);
            }

        }
    }

    public class NvgRenderPass : RenderStage, IDisposable
    {
        public static Nvg Thing;
        readonly NvgRenderer _nvgRenderer;

        public NvgRenderPass(CommandList list, Renderer renderer, string name = null)
        {
            NvgRendererParams rendererParams = new NvgRendererParams
            {
                AdvanceFrameIndexAutomatically = true,
                Device = renderer.Device,
                InitialCommandBuffer = list
            };
            _nvgRenderer = new NvgRenderer(rendererParams, 0);

            Nvg.Create(_nvgRenderer);
        }

        NvgFrame frame;
        
        public NvgRenderPass(Renderer renderer, string name = null)
        {
            NvgRendererParams rendererParams = new NvgRendererParams
            {
                AdvanceFrameIndexAutomatically = true,
                Device = renderer.Device,
                InitialCommandBuffer = renderer.Device.ResourceFactory.CreateCommandList()
            };
            
            
            // Stencil strokes in OpenGL are expensive due to needing all the VertexAttribPtr calls needed to set up the new renderstate that veldrid makes, so disable them, it makes it look worse, but it improves performance 3-4 times 
            if (renderer.Device.BackendType == GraphicsBackend.OpenGL ||
                renderer.Device.BackendType == GraphicsBackend.OpenGLES)
            {
                _nvgRenderer = new NvgRenderer(rendererParams, RenderFlags.Antialias);
            }
            else
            {
                _nvgRenderer = new NvgRenderer(rendererParams, RenderFlags.StencilStrokes | RenderFlags.Antialias);   
            }
            Thing = Nvg.Create(_nvgRenderer);
            frame = new NvgFrame(_nvgRenderer, new NvgFrameBufferParams()
            {
                Framebuffer = Engine.Engine.Renderer.Device.SwapchainFramebuffer,
                GraphicsDevice = Engine.Engine.Renderer.Device,
                List = Engine.Engine.Renderer.Device.ResourceFactory.CreateCommandList()
            });
        }

        
        protected override void Stage(RenderState rendererState, RenderTarget targetFrame, float time, float deltaTime)
        {
            frame.Framebuffer = rendererState.Device.SwapchainFramebuffer;
            
            _nvgRenderer.SetFrame(frame);
            if (WindowClass.Handle.Size.LengthSquared > 0)
            {
                Thing.BeginFrame(targetFrame.Size.X, targetFrame.Size.Y, 1);
                foreach (WeakReference<NvgItem> panel in NvgItem.items)
                {
                    if (panel.TryGetTarget(out NvgItem item))
                    {
                        item.OnDraw(Thing, time, deltaTime);
                    }
                }
                Thing.EndFrame();
            
                _nvgRenderer.Flush();   
            }
        }

        public void Dispose()
        {
            Thing.Dispose();
            _nvgRenderer?.Dispose();
        }
    }
}