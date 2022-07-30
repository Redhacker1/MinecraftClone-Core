using System.Numerics;

namespace Engine.Collision;

public struct Sphere
{
    public float Radius;
    public Vector3 Position;
    public Sphere(float radius, Vector3 Position)
    {
        Radius = radius;
        this.Position = Position;
    }
}