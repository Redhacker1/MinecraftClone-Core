using System;
using System.Numerics;
using Engine.Collision;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Veldrid;

namespace Engine.Rendering.Abstract;

public sealed class Instance3D : MinimalObject
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
        return isVisible && IntersectionHandler.MeshInFrustrum(this, camera.CameraFrustum);
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
    
    AABB RegenAabb()
    {
        
        AABB boundingBox = _baseRenderableElement.GetBoundingBox();
        boundingBox.GetMinMax(out Vector3 minpoint, out Vector3 Maxpoint);

        boundingBox.SetAABB(Vector3.Transform(minpoint, ModelMatrix), Vector3.Transform(Maxpoint, ModelMatrix));
        Boundingbox = boundingBox;
        return boundingBox;
    }

}