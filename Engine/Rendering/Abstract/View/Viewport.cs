using Engine.Utilities.MathLib;

namespace Engine.Rendering.Abstract.View;

public struct Viewport
{
    public Int2 Offset;
    public Int2 Size;
    public float MinDepth;
    public float MaxDepth;
}