using System.Diagnostics;
using System.Numerics;
using Engine.Input;
using NvgExample;
using Silk.NET.Maths;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;

namespace NVGRenderer;

public class NvgItem
{
    internal static List<WeakReference<NvgItem>> items = new List<WeakReference<NvgItem>>();

    readonly WeakReference<NvgItem> _itemWeakRef;
    int entypo;

    public NvgItem()
    {
        _itemWeakRef = new WeakReference<NvgItem>(this);
        items.Add(_itemWeakRef);
    }

    ~NvgItem()
    {
        items.Remove(_itemWeakRef);
    }
    
    public virtual void OnDraw(Nvg nvg, float elapsed, float delta)
    {

        float minVal = 0;
        float maxVal = 1;
        float freq = 1; // oscillations per second
        float avgVal = (minVal + maxVal)/2;
        float amp = (maxVal - minVal) / 2;
        avgVal += amp * MathF.Cos(elapsed * freq / MathF.PI);

        Vector2 lerpfunc = Vector2.Lerp(Vector2.Zero, new Vector2(300, 400), avgVal);

        Vector2D<float> LerpedValue = new Vector2D<float>(lerpfunc.X, lerpfunc.Y);
        nvg.BeginPath();
        nvg.Ellipse(LerpedValue, 100, 300);
        nvg.StrokeColour(Colour.Black);
        nvg.StrokeWidth(3);
        nvg.FillColour(Colour.Indigo);
        nvg.Fill();
        nvg.Stroke();





    }
        
}

public class DemoTest : NvgItem
{
    Demo testDemo;

    public override void OnDraw(Nvg nvg, float elapsed, float delta)
    {
        testDemo ??= new Demo(nvg);

        Vector2 mouseInput = InputHandler.MousePos(0);
        testDemo.Render(mouseInput.X, mouseInput.Y, Engine.Engine.MainFrameBuffer.Size.X, Engine.Engine.MainFrameBuffer.Size.Y, elapsed, false);
    }
        
}

public class PerfMonitor : NvgItem
{
    PerformanceGraph Graph;
    double PrevTime;
    Stopwatch watch;

    public override void OnDraw(Nvg nvg, float elapsed, float delta) 
    {
        watch ??= Stopwatch.StartNew();
        Graph ??= new PerformanceGraph(new Vector2D<uint>(200, 48));

        double t = watch.Elapsed.TotalSeconds;
        double dt = t - PrevTime;
        PrevTime = t;
        Graph.Update((float)dt);
        Graph.Render(5.0f, 5.0f, nvg);
        
    }
        
}