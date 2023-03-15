using System.Collections.Generic;
using System.Diagnostics;
using Engine.Objects.SceneSystem;
using Engine.Rendering.Abstract.View;
using Veldrid;

namespace Engine.Rendering.Abstract.RenderStage;
public struct RenderState
{
    public GraphicsDevice Device;
    public CommandList GlobalCommandList;
    public RenderTarget Target;
}

public abstract class RenderStage
{
    
    

    Stopwatch _stopwatch = Stopwatch.StartNew();
    float prevTime;
    internal void RunStage(RenderState rendererState, RenderTarget target, float curTime,IReadOnlyList<Instance> RenderObjects)
    {
        Stage(rendererState, target, (float)_stopwatch.Elapsed.TotalSeconds, curTime - prevTime, RenderObjects );
        prevTime = curTime;
    }

    protected abstract void Stage(RenderState rendererState, RenderTarget target, float time, float deltaTime, IReadOnlyList<Instance> RenderObjects);
}