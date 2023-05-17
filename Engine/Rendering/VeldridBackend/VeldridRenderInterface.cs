using Engine.Rendering.Abstract;

namespace Engine.Rendering.VeldridBackend;

public class VeldridRenderInterface : BaseRenderInterface
{

    public override BaseBufferTyped<T> CreateBuffer<T>()
    {
        throw new System.NotImplementedException();
    }
}