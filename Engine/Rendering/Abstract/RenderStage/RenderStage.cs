using System;
using System.Diagnostics;
using BepuUtilities;
using Engine.Rendering.Abstract.View;
using Veldrid;

namespace Engine.Rendering.Abstract.RenderStage;
public struct RenderState
{
    public GraphicsDevice Device;
    public CommandList GlobalCommandList;
}

public abstract class RenderStage
{
    
    

    Stopwatch _stopwatch = Stopwatch.StartNew();
    float prevTime;
    internal void RunStage(RenderState rendererState, RenderTarget target)
    {
        float curTime = (float) _stopwatch.Elapsed.TotalMilliseconds * 1000;
        Stage(rendererState, target, (float)_stopwatch.Elapsed.TotalSeconds, curTime - prevTime );
        prevTime = curTime;
    }

    protected abstract void Stage(RenderState rendererState, RenderTarget target, float time, float deltaTime);
}