using Silk.NET.Maths;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Text;

namespace NvgExample
{

    public class PerformanceGraph
    {
        public enum GraphRenderStyle
        {
            Fps,
            Ms,
            Percent
        }
        
        public enum LineStyle : byte
        {
            Fill,
            Stroke
        }

        private LineStyle _style = LineStyle.Stroke;
        private readonly string _name;
        float[] _values;
        Vector2D<uint> boxSize;

        private uint _head;
        public float GraphAverage
        {
            get
            {
                float avg = 0;
                for (uint i = 0; i <  _values.Length; i++)
                {
                    avg += _values[i];
                }
                return avg /  _values.Length;
            }
        }

        Vector2D<float> MinMax
        {
            get
            {
                Vector2D<float> minMaxVal = new Vector2D<float>(float.PositiveInfinity, float.NegativeInfinity);  
                for (uint i = 0; i < _values.Length; i++)
                {
                    minMaxVal.X = MathF.Min(minMaxVal.X, _values[i]);
                    minMaxVal.Y = MathF.Max(minMaxVal.Y, _values[i]);
                }

                return minMaxVal;
            }
        }

        public PerformanceGraph(Vector2D<uint> graphSize)
        {
            Resize(graphSize);
        }

        public void Update(float frameTime)
        {
            _head = (_head  + 1) % (uint)_values.Length;
            _values[_head] = frameTime;
        }
        
        float GetValue(uint index)
        {
            index %= (uint)_values.Length;
            return _values[index];
        }

        Vector2D<float> _range;


        public void Resize(Vector2D<uint> size)
        {
            boxSize = size;
            float[] oldArray = _values;
            _values = new float[size.X / 2];
            if (oldArray != null)
            {
                int TransferSize = Math.Min(oldArray.Length,_values.Length);
                int TransferStart = Math.Max(TransferSize - oldArray.Length, 0);
                Array.Copy(oldArray, TransferStart, _values, 0, TransferSize);   
            }

            _head = 0;

        }




        void DrawBackGroundRect(Nvg nvg, float x, float y)
        {
            nvg.BeginPath();
            nvg.Rect(x, y, boxSize.X, boxSize.Y);
            nvg.FillColour(nvg.Rgba(0, 0, 0, 128));
            nvg.Fill();
        }

        void DrawLines(Nvg nvg, float x, float y)
        {
            float XStepSize = (float)boxSize.X / _values.Length;
            float startPoint = NormalizeData(GetValue(_head), _range.X, _range.Y);
            
            nvg.BeginPath();
            
            nvg.MoveTo(x, startPoint * boxSize.Y);
            for (uint graphData = _head; graphData < _values.Length + _head; graphData++)
            {
                var pointValue = GetValue(graphData);
                var height = NormalizeData(pointValue, _range.X, _range.Y);

                if (height >= 1 || height <= 0)
                {
                    _range = MinMax;
                    height = NormalizeData(pointValue, _range.X, _range.Y);
                }
                nvg.LineTo(XStepSize * (graphData - _head) + (x), (y)  + height * (boxSize.Y));
            }


            if (_style == LineStyle.Fill)
            {
                nvg.LineTo(x + boxSize.X, y + boxSize.Y);
                nvg.LineTo(x, y + boxSize.Y);
                nvg.LineTo(x, startPoint * boxSize.Y);
                nvg.FillColour(Colour.Red);
                nvg.Fill();
            }
            else
            {
                nvg.StrokeWidth(1f);
                nvg.StrokeColour(Colour.Red);
                nvg.Stroke();
            }
        }

        bool normalize = true;
        public void Render(float x, float y, Nvg nvg)
        {
            var fontIndex = nvg.FindFont("sans-bold");
            if (fontIndex == -1)
            {
                fontIndex = nvg.CreateFont("sans-bold", "./fonts/Roboto-Bold.ttf");
            }
            

            if (normalize)
            {
                _range = MinMax;
                normalize = false;
            }
            DrawBackGroundRect(nvg, x, y);
            DrawLines(nvg, x, y);
            DrawText(nvg, x, y);
        }

        void DrawText(Nvg nvg, float x, float y)
        {
            
            var minmax = MinMax;
            
            nvg.FontSize(11);
            nvg.FontFace("sans-bold");
            nvg.FillColour(new Colour(57, 255, 0));
            nvg.TextAlign(Align.Top);
            
            TextHelper(nvg, $"Maximum: {minmax.Y * 1000}", new Vector2D<float>(x + boxSize.X, y - 5.5f));

            TextHelper(nvg, $"Average: {GraphAverage * 1000}", new Vector2D<float>(x + boxSize.X, y - 5f + (boxSize.Y / 2f) - 5.5f));
            
            TextHelper(nvg, $"Minimum: {minmax.X * 1000}", new Vector2D<float>(x + boxSize.X, y - 16f + boxSize.Y));
        }

        void TextHelper(Nvg nvg, string text, Vector2D<float> location)
        {
            nvg.TextBounds(0, 0, text, out Rectangle<float> renderBounds);
            nvg.Text(location.X, location.Y + renderBounds.Size.Y - 5.5F, text);
        }


        float NormalizeData(float input, float min, float max)
        {
            return (input - min) / (max - min);
        }
    }
}
