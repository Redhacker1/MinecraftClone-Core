using Engine.Rendering.VeldridBackend;

namespace Engine.Rendering.Abstract;

/// <summary>
/// Standardized interface for interacting with 
/// </summary>
public abstract class BaseRenderInterface
{

    public abstract BaseBufferTyped<T> CreateBuffer<T>() where T : unmanaged;


}