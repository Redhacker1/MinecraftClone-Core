using System.Numerics;
using Engine.Collision;
using Engine.Rendering.Abstract;
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using Silk.NET.Maths;
using Veldrid;

namespace LineTest;

public struct Line : IDisposable
{

    public static readonly object locker = new object();
    public static readonly List<Line> _lines = new List<Line>();
    public Vector3 Point1;
    public Vector3 Point2;
    public LineFrag LineData;

    public Line()
    {
        Point1 = default;
        Point2 = default;
        LineData = default;
        lock (locker)
        {
            _lines.Add(this);
        }
        
    }

    static Matrix4x4 ModelMatrix => Matrix4x4.CreateFromQuaternion(Quaternion.Identity)
                                    * Matrix4x4.CreateScale(1)
                                    * Matrix4x4.CreateTranslation(Camera.MainCamera != null ? -Camera.MainCamera.Pos : Vector3.Zero);

    public void Dispose()
    {
        lock (locker)
        {
            _lines.Remove(this);
        }
    }

}

public struct LineFrag
{
    float uLineWidth;
    Vector4 Color;
    float BlendFactor; //1.5..2.5


    public LineFrag()
    {
        uLineWidth = default;
        Color = default;
        BlendFactor = 1.5f;
    }

}


class LineRenderer : RenderPass
{    
    static Vector2D<int> ScreenResolution => WindowClass.Handle.Size;

    public LineRenderer(CommandList _list, Renderer renderer) : base(_list, renderer)
    {
            
    }

    public LineRenderer(Renderer renderer) : base(renderer)
    {
            
    }

    protected override void Pass(CommandList list, List<Instance3D> instances, ref CameraInfo cameraInfo)
    {
    
        Frustum frustum = Camera.MainCamera.GetViewFrustum(out _);
        AABB boundingBox = new AABB();


        lock (Line.locker)
        {
            foreach (Line line in Line._lines)
            {
                // Calculate the AABB of each line NOTE: this may hamper performance,
                // Especially when we move to have this all instanced
                Vector3 min = Vector3.Min(line.Point1, line.Point2);
                Vector3 max = Vector3.Max(line.Point1, line.Point2);
                boundingBox.Origin = (min + max) * .5f;
                boundingBox.SetExtents(max - boundingBox.Origin);
                bool result = IntersectionHandler.aabb_to_frustum(ref boundingBox, frustum);


                if (result)
                {
                    // do Drawing stuff in here
                    
                }

            }
        }
    }
}

