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

    public void PrepareForIntegration(float dt)
    {
        throw new System.NotImplementedException();
    }
    

    public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
        BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
    {
        
        
        throw new System.NotImplementedException();
    }

    public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;
    public readonly bool IntegrateVelocityForKinematics => false;

    Simulation IBepuIntegration.GetSimulation
    {
        get => _getSimulation;
        set => _getSimulation = value;
    }
}