using System.Diagnostics;
using BepuUtilities;
using Veldrid;

namespace Engine.Rendering.Abstract.RenderStage;

public struct Frame
{
    public Utilities.MathLib.Int2 Size;
    public Int2 Offset;
    public Framebuffer Buffer;
}

public struct RenderState
{
    public GraphicsDevice Device;
    public CommandList GlobalCommandList;
}

public abstract class RenderStage
{
    
    

    Stopwatch _stopwatch = Stopwatch.StartNew();
    float prevTime;
    internal void RunStage(RenderState rendererState, Frame TargetFrame)
    {

        float curTime = (float) _stopwatch.Elapsed.TotalMilliseconds;
        Stage(rendererState, TargetFrame, curTime, curTime - prevTime );
        prevTime = curTime;
    }

    protected abstract void Stage(RenderState rendererState, Frame TargetFrame, float time, float deltaTime);
}