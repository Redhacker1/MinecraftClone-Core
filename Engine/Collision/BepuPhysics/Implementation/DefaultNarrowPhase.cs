using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;

namespace Engine.Collision.BEPUPhysics.Implementation;

public struct DefaultNarrowPhase : IEngineNarrowPhase
{
    Simulation _getSimulation;


    public void Initialize(Simulation simulation)
    {
        ((IBepuIntegration) this).GetSimulation = simulation;
    }

    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        // For now lets keep this simple.
        return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
    }

    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold,
        out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        throw new System.NotImplementedException();
    }

    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
    {
        return true;
    }
    
    public void Dispose()
    {
        
    }

    Simulation IBepuIntegration.GetSimulation
    {
        get => _getSimulation;
        set => _getSimulation = value;
    }
}