using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;
using Engine.Objects.SceneSystem;
using Engine.Rendering.Abstract;
using Engine.Rendering.Abstract.RenderStage;
using Engine.Rendering.Abstract.View;
using Engine.Rendering.VeldridBackend;
using Engine.Utilities.MathLib;
using Engine.Windowing;
using NVGRenderer.Rendering;
using SilkyNvg;
using Veldrid;

namespace NVGRenderer
{
    static class EntryPoint
    {
        public static void Main()
        {
            WindowParams windowParams = new WindowParams()
            {
                Location = new Int2(100, 100),
                Size = new Int2(1280, 720),
                Name = "Default window",
            };
            Init.InitEngine(ref windowParams, new NvgRendererDemo());
        }
    }

    class NvgRendererDemo : GameEntry
    {
        Camera cam = new Camera(new Transform(), -Vector3.UnitZ, Vector3.UnitY,1.3333334F, true );
        NvgItem DemoTest;
        NvgItem PerfGraph;
        NvgRenderPass pass;
        public static Scene Scene;
        
        protected override void GameStart()
        {
            pass = new NvgRenderPass(Engine.Engine.Renderer);
            Scene = new Scene();
            
            Scene.AddStage(pass);

            DemoTest = new DemoTest();
            PerfGraph = new PerfMonitor();
        }
        
        protected override void OnRender(float deltaT)
        {
            base.OnRender(deltaT);
            Scene?.Render(Camera.MainCamera, Engine.Engine.MainFrameBuffer, deltaT);
        }
    }

    class GameObject : EngineObject
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
            
            
            // Stencil strokes in OpenGL are expensive due to needing all the VertexAttribPtr calls needed to set up the new render-state that veldrid makes, so disable them, it makes it look worse, but it improves performance 3-4 times 
            RenderFlags flags = RenderFlags.Antialias;// | RenderFlags.StencilStrokes;
            if (renderer.Device.BackendType is not GraphicsBackend.OpenGL and not GraphicsBackend.OpenGLES)
            {
               //flags |= RenderFlags.StencilStrokes;
            }
            _nvgRenderer = new NvgRenderer(rendererParams, flags);
            Thing = Nvg.Create(_nvgRenderer);
            
            frame = new NvgFrame(_nvgRenderer, new NvgFrameBufferParams()
            {
                Framebuffer = Engine.Engine.MainFrameBuffer,
                GraphicsDevice = Engine.Engine.Renderer.Device,
                List = Engine.Engine.Renderer.Device.ResourceFactory.CreateCommandList()
            });
        }

        protected override void Stage(RenderState rendererState, RenderTarget target, float time, float deltaTime, IReadOnlyList<Instance> renderObjects)
        {
            if (WindowEvents.Handle.Bounds.Size.LengthSquared() > 0)
            {
                _nvgRenderer.SetFrame(frame);
                Thing?.BeginFrame(Engine.Engine.MainFrameBuffer.Size.X, Engine.Engine.MainFrameBuffer.Size.Y, 1);
                foreach (WeakReference<NvgItem> panel in NvgItem.items)
                {
                    if (panel.TryGetTarget(out NvgItem item))
                    {
                        item.OnDraw(Thing, time, deltaTime);
                    }
                }
                Thing?.EndFrame();
            
                _nvgRenderer?.Flush();   
            }
        }

        public void Dispose()
        {
            Thing.Dispose();
            _nvgRenderer?.Dispose();
        }
        
        
    }
}