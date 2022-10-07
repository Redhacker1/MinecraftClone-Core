using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace NVGRenderer.Rendering.Shaders;

[StructLayout(LayoutKind.Explicit)]
internal struct VertUniforms
{

    [FieldOffset(0)]

    public Vector2D<float> ViewSize;

}