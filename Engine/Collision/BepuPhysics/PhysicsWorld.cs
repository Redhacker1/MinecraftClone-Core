using System;
using BepuPhysics;
using BepuUtilities.Memory;

namespace Engine.Collision.BEPUPhysics;

public class PhysicsWorld : IDisposable
{
    BepuPhysics.Simulation _simulation;
    BufferPool MemoryPool;


    public void Create<TEngineNarrowPhase, TEnginePoseCallbacks>(TEngineNarrowPhase narrowPhase, TEnginePoseCallbacks poseIntegrator) where TEngineNarrowPhase : struct, IEngineNarrowPhase where TEnginePoseCallbacks : struct, IEnginePoseIntegrator
    {
        MemoryPool = new BufferPool();
        _simulation = Simulation.Create(MemoryPool, narrowPhase, poseIntegrator, new SolveDescription(8, 1));
    }

    internal void RegisterBody(bool isStatic)
    {
        
    }

    /// <summary>
    /// Runs on the level in a fixed timestep.
    /// </summary>
    /// <param name="delta"></param>
    public void Simulate(float delta)
    {
        PreSimulate(delta);
        // Engine simulation loop here. 
        _simulation.Timestep(delta);
        PostSimulate(delta);
        
    }

    /// <summary>
    /// User defined simulation stuff
    /// </summary>
    /// <param name="delta"></param>
    protected virtual void PreSimulate(float delta)
    {
        
    }

    protected virtual void PostSimulate(float delta)
    {
        
    }

    public void Dispose()
    {
    }
}