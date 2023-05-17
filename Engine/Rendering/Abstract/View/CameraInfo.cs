using System.Numerics;
using Engine.Collision.Simple;
using Engine.MathLib;

namespace Engine.Rendering.Abstract;

public struct CameraInfo
{
    public readonly Frustum CameraFrustum;
    public readonly Camera Self;
    public Vector3 Up;
    public Vector3 Forward;
    public Vector3 Right;
    public Transform CameraTransform;


    public CameraInfo(Camera camera)
    {
        Up = camera.Up;
        Forward = camera.Forward;
        Right = camera.Right;
        CameraTransform = camera.LocalTransform;
        Self = camera;
        CameraFrustum = camera.GetViewFrustum(out _);
    }
}