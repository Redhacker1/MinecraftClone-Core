namespace Engine.Rendering.Abstract;

// TODO: Maybe this should be an interface? Is the base buffer for all engine buffer types in rendering
/// <summary>
/// This is the lowest level buffer type in engine, and all the other buffer types are wrappers and types around it.
/// </summary>
public class GraphicsBuffer
{
    /// <summary>
    /// The Render Interface where this data comes from. 
    /// </summary>
    protected BaseRenderInterface RenderInterface;
    
    /// <summary>
    /// The buffer usage that describe what this buffer can be used for and what "traits" it allows for
    /// </summary>
    public BufferType Usage;
    
    /// <summary>
    /// The size, in bytes of the buffer
    /// </summary>
    public long Length;
    
    
}