﻿using System.Diagnostics;
using System.Numerics;
using Engine.Input;
using Engine.Windowing;
using NvgExample;
using Silk.NET.Maths;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Text;

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

    float TotalTime = 0;
    public virtual void OnDraw(Nvg nvg, float timeElapsed)
    {
        
        TotalTime += timeElapsed;
        
        float minVal = 0;
        float maxVal = 1;
        float freq = 1; // oscillations per second
        float avgVal = (minVal + maxVal)/2;
        float amp = (maxVal - minVal) / 2;
        avgVal += amp * MathF.Cos(TotalTime * freq * 10 / MathF.PI);

        Console.WriteLine(avgVal);
        
        

        var lerpfunc = Vector2.Lerp(Vector2.Zero, new Vector2(300, 400), avgVal);

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
    double PrevTime;
    Stopwatch watch;

    public override void OnDraw(Nvg nvg, float timeElapsed)
    {
        watch ??= Stopwatch.StartNew();
        testDemo ??= new Demo(nvg);
        
        double t = watch.Elapsed.TotalSeconds;
        double dt = t - PrevTime;
        PrevTime = t;

        Vector2 mouseInput = InputHandler.MousePos(0);
        testDemo.Render(mouseInput.X, mouseInput.Y, WindowClass.Handle.Size.X, WindowClass.Handle.Size.Y, (float)t, false);
        //watch.Restart();
    }
        
}

public class PerfMonitor : NvgItem
{
    PerformanceGraph Graph;
    double PrevTime;
    Stopwatch watch;

    public override void OnDraw(Nvg nvg, float timeElapsed) 
    {
        watch ??= Stopwatch.StartNew();
        Graph ??= new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Ms, "FPS");

        double t = watch.Elapsed.TotalSeconds;
        double dt = t - PrevTime;
        PrevTime = t;
        Graph.Update((float)dt);
        Graph.Render(5.0f, 5.0f, nvg);
        
    }
        
}