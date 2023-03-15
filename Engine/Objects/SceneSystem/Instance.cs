using Engine.Renderable;
using Engine.Rendering.VeldridBackend;
namespace Engine.Objects.SceneSystem;

public abstract class Instance : EngineObject
{
    public Material InstanceMaterial;
    public bool isVisible = true;
    internal BaseRenderable _baseRenderableElement;
}