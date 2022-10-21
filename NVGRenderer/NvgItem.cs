using System.Diagnostics;
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
        
        NvgRenderPass.thing.CreateFont("icons", "./fonts/entypo.ttf");
    }

    ~NvgItem()
    {
        items.Remove(_itemWeakRef);
    }
    public virtual void OnDraw(Nvg nvg)
    {
        nvg.BeginPath();
        nvg.Ellipse(new Vector2D<float>(400, 300), 100, 300);
        nvg.StrokeColour(Colour.Black);
        nvg.StrokeWidth(1);
        nvg.Stroke();

        nvg.BeginPath();
        nvg.FontSize(32);
        nvg.FontFace("icons");
        nvg.FillColour(Colour.Indigo);
        nvg.Text(new Vector2D<float>(400, 600), "Hello");





    }
        
}

public class DemoTest : NvgItem
{
    Demo testDemo;
    double PrevTime;
    Stopwatch watch;

    public override void OnDraw(Nvg nvg)
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