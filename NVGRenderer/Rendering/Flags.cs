namespace NVGRenderer.Rendering
{

    [Flags]
    public enum RenderFlags : byte
    {

        Antialias = 1 << 0,
        StencilStrokes = 1 << 1,
        Debug = 1 << 2,
        TriangleListFill = 1 << 3

    }
}