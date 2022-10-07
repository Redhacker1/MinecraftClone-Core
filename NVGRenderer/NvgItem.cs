using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;

namespace NVGRenderer;

public class NvgItem
{
    internal static List<WeakReference<NvgItem>> items = new List<WeakReference<NvgItem>>();

    readonly WeakReference<NvgItem> _itemWeakRef;

    public NvgItem()
    {
        _itemWeakRef = new WeakReference<NvgItem>(this);
        items.Add(_itemWeakRef);
    }

    ~NvgItem()
    {
        items.Remove(_itemWeakRef);
    }
    public virtual void OnDraw(Nvg nvg)
    {
        nvg.BeginPath();
        nvg.Rect(100,100, 120,30);
        nvg.FillColour(nvg.Rgba(255,192,0,255));
        nvg.Fill();
    }
        
}