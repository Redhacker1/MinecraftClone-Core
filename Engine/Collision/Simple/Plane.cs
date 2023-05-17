using System.Numerics;
using System.Runtime.Intrinsics;

namespace Engine.Collision.Simple;

public struct Plane
{

    internal Vector128<float> SIMDnormal;
    public Vector3 Normal
    {
        get => SIMDnormal.AsVector3();
        set => SIMDnormal = value.AsVector128();
    }
    public float Offset;
    public Plane(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        SIMDnormal = default;
        SIMDnormal = Vector3.Normalize(Vector3.Cross(p1-p0, p2 - p1)).AsVector128();
        Offset = Vector3.Dot(SIMDnormal.AsVector3(), p0);
    }
}