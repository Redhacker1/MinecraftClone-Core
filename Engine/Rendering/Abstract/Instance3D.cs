using System.Numerics;
using Engine.Collision.Simple;
using Engine.MathLib;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Veldrid;

namespace Engine.Rendering.Abstract;

public sealed class Instance3D : EngineObject
{

    public bool isVisible = true;
    internal BaseRenderableUntyped _baseRenderableElement;
    internal Material ModelMaterial;
    internal AABB Boundingbox;
    public Instance3D(BaseRenderableUntyped baseRenderableElement, Material material)
    {
        _baseRenderableElement = baseRenderableElement;
        ModelMaterial = material;
        RegenAabb();
    }

    /// <summary>
    /// Logic to decide if the object should be rendered, called for each item, each frame;
    /// </summary>
    /// <returns>Whether the object should be rendered</returns>
    public bool ShouldRender(ref CameraInfo camera)
    {
        return isVisible && IntersectionHandler.MeshInFrustum(this, camera.CameraFrustum);
    }

    public void GetInstanceAabb(out AABB boundingBox, Vector3 frustumCamerapos = default)
    {
        boundingBox = Boundingbox;
        boundingBox.Origin -= frustumCamerapos;
    }

    protected override void OnTransformUpdated()
    {
        RegenAabb();
    }
    
    void RegenAabb()
    {
        
        AABB boundingBox = _baseRenderableElement.GetBoundingBox();
        boundingBox.GetMinMax(out Vector3 minpoint, out Vector3 Maxpoint);

        Matrix4x4 localMatrix = LocalTransform.AsMatrix4X4();
        boundingBox.SetAABB(Vector3.Transform(minpoint, localMatrix), Vector3.Transform(Maxpoint, localMatrix));
        Boundingbox = boundingBox;
    }

    public void SetAABB(AABB boundingBox)
    {
        Boundingbox = boundingBox;
    }
}