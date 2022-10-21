using System.Numerics;
using NVGRenderer.Rendering.Pipelines;
using Veldrid;

namespace NVGRenderer.Rendering.Draw;



public struct DrawCall
{
    public ResourceSetData Set;
    public Pipeline Pipeline;
    public Vector2 Scissor;
    public uint Offset;
    public uint Count;
}