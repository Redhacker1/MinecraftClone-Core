using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using Engine.MathLib;

namespace Engine.Collision.BEPUPhysics.Implementation.BodyTypes;

public sealed class BoxCollider : PhysicsBody
{
    Vector3 extents;
    internal BoxCollider(PhysicsWorld world, Transform transform, Vector3 extents, float mass = 0f, float inertia = -1f) : base( world, transform, mass, inertia )
    {
        this.extents = extents;
        this.Mass = mass;
        this.Inertia = inertia;
    }

    public BoxCollider(PhysicsWorld world ,Vector3 extents) : this(world, new Transform(), extents)
    {
        
    }

    internal override bool GetBody(Simulation simulation, out BodyDescription description)
    {

        if (simulation.Bodies.BodyExists(bodyHandle: handle))
        {
            Vector3 scaledExtents = extents * _transform.Scale;
            Box box = new Box(scaledExtents.X, scaledExtents.Y, scaledExtents.Z);
        
            description = BodyDescription.CreateDynamic(new RigidPose(_transform.Position, _transform.Rotation), box.ComputeInertia(Mass), simulation.Shapes.Add(box), 0.01f);
            return false;
        }
        else
        {
            description = default;
            return false;
        }
    }
}