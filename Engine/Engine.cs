using System;
using Engine.Rendering.Abstract.View;
using Engine.Rendering.VeldridBackend;
using Veldrid;

namespace Engine;

/// <summary>
/// A static class that holds all of the engine specific state needed, either temporarily or permanently.
/// </summary>
public static class Engine
{
    public static GameEntry CurrentGame { get; internal set; }
    
    public static RenderTarget MainFrameBuffer { get; internal set; }

    public static Renderer Renderer { get; internal set; }
    
    public static Action<float> OnRender = _ =>
    {
        
    };

    public static bool IsRenderThread => Windowing.WindowEvents.IsRenderThread();
}