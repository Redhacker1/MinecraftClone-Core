using System.Numerics;
using Engine.Collision;

namespace Engine.Rendering.Abstract;

public struct CameraInfo
{
    public Frustum CameraFrustum;
    
    public Camera Self;

    public Vector3 Up;
    public Vector3 Forward;
    public Vector3 Right;
    public Vector3 CameraPos;
    public Quaternion CameraRotation;


    public CameraInfo(Camera camera)
    {
        Up = camera.Up;
        Forward = camera.Forward;
        Right = camera.Right;
        CameraPos = camera.Pos;
        CameraRotation = camera.Rotation;
        Self = camera;
        CameraFrustum = camera.GetViewFrustum(out _);
    }
}