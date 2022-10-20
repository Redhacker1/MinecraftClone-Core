using System.Numerics;
using Veldrid;

namespace NVGRenderer.Rendering.Draw;

public struct DrawCall
{
    public ResourceSet Set;
    public Pipeline Pipeline;
    public uint Offset;
    public uint Count;
}