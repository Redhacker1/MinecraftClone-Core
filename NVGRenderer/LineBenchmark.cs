using Engine.Windowing;
using Silk.NET.Maths;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;

namespace NVGRenderer;

class LineBenchmark : NvgItem
{

    static Random rand = new Random();
    void BenchMark(int width, int height, Nvg _nvgContext)
    {
        
        _nvgContext.StrokeWidth(1f);

        Span<byte> colour = stackalloc byte[4];
        for (int i = 0; i < 9000; i++)
        {
            float x1 = rand.NextSingle() * width;
            float x2 = rand.NextSingle() * width;
            float y1 = rand.NextSingle() * height;
            float y2 = rand.NextSingle() * height;

            rand.NextBytes(colour);
                
            _nvgContext.StrokeColour(new Colour(colour[0], colour[1], colour[2], colour[3]));
            _nvgContext.BeginPath();
            _nvgContext.MoveTo(x1, y1);
            _nvgContext.LineTo(x2, y2);
            _nvgContext.Stroke();
        }
    }

    public override void OnDraw(Nvg nvg, float elapsed,  float delta)
    {
        BenchMark(Engine.Engine.MainFrameBuffer.Size.X, Engine.Engine.MainFrameBuffer.Size.Y, nvg);
    }
}