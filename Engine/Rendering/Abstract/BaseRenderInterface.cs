using Engine.Rendering.VeldridBackend;
using Silk.NET.Windowing;

namespace Engine.Rendering.Abstract;

/// <summary>
/// Standardized interface for interacting with 
/// </summary>
public abstract class BaseRenderInterface
{
    protected IView BackingTarget;
    protected BaseRenderInterface(IView view)
    {
        BackingTarget = view;
    }

    public abstract BaseBufferTyped<T> CreateBuffer<T>() where T : unmanaged;


}