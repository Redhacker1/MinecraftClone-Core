﻿using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.MathLib;
using Engine.Rendering.Abstract;
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using NVGRenderer;
using NVGRenderer.Rendering;
using Silk.NET.Maths;
using SilkyNvg;
using Veldrid;

class EntryPoint
{
    public static void Main()
    {
        Init.InitEngine(0,0, 1920, 1080, "NVG", new NvgRendererDemo());
    }
}

class NvgRendererDemo : GameEntry
{
    NvgItem Item;
    protected override void GameStart()
    {
        Camera.MainCamera = new Camera(new Transform(), Vector3.UnitX, Vector3.UnitY, 1.7777778F, true);
        NVGRenderPass pass = new NVGRenderPass(WindowClass.Renderer, "Rendering NVG Stuff");
        WindowClass.Renderer.AddPass(1, pass);

        Item = new NvgItem();

    }
}

class NVGRenderPass : RenderPass
{
    public Nvg thing;
    NvgRenderer Renderer;
    public NVGRenderPass(CommandList list, Renderer renderer, string name = null) : base(list, renderer, name)
    {
        NvgRendererParams rendererParams = new NvgRendererParams()
        {
            AdvanceFrameIndexAutomatically = true,
            Device = renderer.Device,
            FrameCount = 10u,
            InitialCommandBuffer = list
        };
        Renderer = new NvgRenderer(rendererParams, 0);
        Nvg.Create(Renderer);
    }

    public NVGRenderPass(Renderer renderer, string name = null) : base(renderer, name)
    {
        NvgRendererParams rendererParams = new NvgRendererParams()
        {
            AdvanceFrameIndexAutomatically = true,
            Device = renderer.Device,
            FrameCount = 10u,
            InitialCommandBuffer = renderer.Device.ResourceFactory.CreateCommandList()
        };
        Renderer = new NvgRenderer(rendererParams, 0);
        thing = Nvg.Create(Renderer);
    }

    protected override void Pass(CommandList list, List<Instance3D> instances, ref CameraInfo camera)
    {
        thing.BeginFrame(new Vector2D<float>(WindowClass.Handle.Size.X, WindowClass.Handle.Size.Y), 1);
        foreach (WeakReference<NvgItem> panel in NvgItem.items)
        {
            if (panel.TryGetTarget(out NvgItem item))
            {
                item.OnDraw(thing);
            }
        }
        thing.EndFrame();
        backingRenderer.Device.SubmitCommands(Renderer.CurrentCommandBuffer);
    }
}