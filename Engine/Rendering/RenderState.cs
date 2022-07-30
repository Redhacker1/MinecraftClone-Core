namespace Engine.Rendering;

public class RenderState
{
    public RenderState CurrentState { get; } = new RenderState();

    /// <summary>
    /// The Framerate at this point.
    /// </summary>
    public uint FrameRate;
    
    /// <summary>
    /// Number of passes made this frame
    /// </summary>
    public uint PassCount;
    
    /// <summary>
    /// The amount of triangles drawn this frame.
    /// </summary>
    public long TriCount;
    
    
}