using System.Numerics;
using System.Runtime.Intrinsics;

namespace Engine.Collision.Simple;

public struct Sphere
{
    public readonly float Radius;

    internal Vector128<float> PosSimd;
    public Vector3 Position { get => PosSimd.AsVector3(); set => PosSimd = value.AsVector128(); }
    public Sphere(float radius, Vector3 position)
    {
        PosSimd = position.AsVector128();
        Radius = radius;
    }
    
    public Sphere(Vector128<float> definition)
    {
        Radius = definition.GetElement(3);
        PosSimd = definition.WithElement(3, 0);
    }
}