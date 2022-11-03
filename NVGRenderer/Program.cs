using System.Diagnostics;
using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering.Abstract;
using Engine.Rendering.Abstract.RenderStage;
using Engine.Rendering.Abstract.View;
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using NVGRenderer.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
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
        NvgItem DemoTest;
        NvgItem PerfGraph;
        NvgRenderPass pass;
        protected override void GameStart()
        {
            Camera.MainCamera = new Camera(new Transform(), Vector3.UnitX, Vector3.UnitY, 1.7777778F, true);
            pass = new NvgRenderPass(Engine.Engine.Renderer, "Rendering NVG Stuff");
            Engine.Engine.MainFrameBuffer.AddPass(1, pass);

            DemoTest = new DemoTest();
            PerfGraph = new PerfMonitor();
            this.PinnedObject = new BaseLevel();
            this.PinnedObject.AddEntityToLevel(new GameObject());

        }

        protected override void GameEnded()
        {
            pass.Dispose();
            base.GameEnded();
        }
    }

    class GameObject: EngineObject
    {
        public GameObject()
        {
            this.Ticks = true;
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

    class NvgRenderPass : RenderStage, IDisposable
    {
        public static Nvg Thing;
        readonly NvgRenderer _nvgRenderer;
        NvgFrame _frame;
        
        public NvgRenderPass(CommandList list, Renderer renderer, string name = null)
        {
            NvgRendererParams rendererParams = new NvgRendererParams
            {
                AdvanceFrameIndexAutomatically = true,
                Device = renderer.Device,
                InitialCommandBuffer = list
            };
            _nvgRenderer = new NvgRenderer(rendererParams, RenderFlags.StencilStrokes | RenderFlags.Antialias);
            Debug.Assert(_nvgRenderer.EdgeAntiAlias);
            
            Console.WriteLine(FlagHelper.HasFlag((int)(RenderFlags.StencilStrokes | RenderFlags.Antialias), (int)RenderFlags.StencilStrokes));
            
            Nvg.Create(_nvgRenderer);
        }

        public NvgRenderPass(Renderer renderer, string name = null)
        {
            NvgRendererParams rendererParams = new NvgRendererParams
            {
                AdvanceFrameIndexAutomatically = true,
                Device = renderer.Device,
                InitialCommandBuffer = renderer.Device.ResourceFactory.CreateCommandList()
            };
            
            
            _nvgRenderer = new NvgRenderer(rendererParams, RenderFlags.StencilStrokes | RenderFlags.Antialias);
            Thing = Nvg.Create(_nvgRenderer);
            _frame = new NvgFrame(_nvgRenderer, new NvgFrameBufferParams()
            {
                Framebuffer = renderer.Device.SwapchainFramebuffer
            });

            WindowClass.Handle.Resize += _ =>
            {
                _frame.Framebuffer = renderer.Device.SwapchainFramebuffer;
            };


        }

        
        protected override void Stage(RenderState rendererState, RenderTarget TargetFrame, float time, float deltaTime)
        {
            _nvgRenderer.CurrentCommandBuffer.Begin();
            Thing.BeginFrame(new Vector2D<float>(WindowClass.Handle.Size.X, WindowClass.Handle.Size.Y), 1f);
            foreach (WeakReference<NvgItem> panel in NvgItem.items)
            {
                if (panel.TryGetTarget(out NvgItem item))
                {
                    item.OnDraw(Thing, deltaTime);
                }
            }
            Thing.EndFrame();
            
            _nvgRenderer.Flush();
            _nvgRenderer.CurrentCommandBuffer.End();
            _nvgRenderer.Device.SubmitCommands(_nvgRenderer.CurrentCommandBuffer);
        }

        public void Dispose()
        {
            Thing.Dispose();
            _nvgRenderer?.Dispose();
        }
    }
}