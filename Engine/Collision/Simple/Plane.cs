using System.Numerics;

namespace Engine.Collision.Simple;

public struct Plane
{
    public Vector3 Normal;
    public float Offset;
    public Plane(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Normal = Vector3.Normalize(Vector3.Cross(p1-p0, p2 - p1));
        Offset = Vector3.Dot(Normal, p0);
    }
}