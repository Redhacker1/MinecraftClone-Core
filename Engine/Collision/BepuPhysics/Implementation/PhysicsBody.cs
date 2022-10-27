using System.Numerics;
using BepuPhysics;
using Engine.MathLib;

namespace Engine.Collision.BEPUPhysics.Implementation;

public abstract class PhysicsBody
{
    protected PhysicsWorld world;
    internal BodyHandle handle;

    Vector3 _scale;

    public Transform _transform
    {
        get
        {
            var success = GetPose(out RigidPose pose);
            if (success)
            {
                return new Transform(pose.Position, _transform.Rotation, _scale);
            }
            else
            {
                return default;
            }
        }
        set
        {
            var success = GetReference(out BodyReference bodyref);
            bodyref.Pose.Orientation = value.Rotation;
            bodyref.Pose.Position = value.Position;
            if (_scale != value.Scale)
            {
                _scale = value.Scale;
                GetBody(world.Simulation, out BodyDescription description);
            }
        }
    }
    
    public float Mass;
    public float Inertia;


    public bool IsValidBody => world != null && world.Simulation.Bodies.BodyExists(handle);

    protected PhysicsBody(PhysicsWorld simulation, Transform transform, float mass, float inertia = -1f)
    {
        world = simulation;
        Mass = mass;
        Inertia = inertia;
        _scale = transform.Scale;
    }


    internal abstract bool GetBody(Simulation simulation, out BodyDescription description);

    protected bool GetReference( out BodyReference bodyReference)
    {
        if (IsValidBody == false)
        {
            bodyReference = default;
            return false;
        }

        bodyReference = world.Simulation.Bodies.GetBodyReference(handle);
        
        return true;
    }
    
    protected bool GetPose( out RigidPose bodyPose)
    {
        if (GetReference(out BodyReference reference))
        {
            bodyPose = default;
            return false;
        }
        bodyPose = reference.Pose;
        
        return true;
    }

    /// <summary>
    /// Only remove if we are finished with the handle, this can probably be fined GCed
    /// </summary>
    ~PhysicsBody()
    {
        world.Simulation.Bodies.Remove(handle);
    }
    
    

}