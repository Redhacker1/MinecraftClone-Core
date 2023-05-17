using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace Engine.Collision.BEPUPhysics.Implementation;

public struct DefaultPoseIntegrator : IEnginePoseIntegrator
{
    Simulation _getSimulation;

    public void Initialize(Simulation simulation)
    {
        ((IBepuIntegration) this).GetSimulation = simulation;
    }
    
    
    Vector3Wide _gravityWideDt;
    public Vector3 Gravity = -Vector3.UnitY * 9.8f;

    public DefaultPoseIntegrator()
    {
        _getSimulation = null;
        _gravityWideDt = default;
    }


    public void PrepareForIntegration(float dt)
    {
        //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
        _gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
    }
    
    
    public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
    {
        velocity.Linear += _gravityWideDt;
    }

    public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;
    public readonly bool IntegrateVelocityForKinematics => true;

    Simulation IBepuIntegration.GetSimulation
    {
        get => _getSimulation;
        set => _getSimulation = value;
    }
}