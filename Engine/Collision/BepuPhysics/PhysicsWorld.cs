using System;
using BepuPhysics;
using BepuUtilities.Memory;
using Engine.Collision.BEPUPhysics.Implementation;

namespace Engine.Collision.BEPUPhysics;

public class PhysicsWorld : IDisposable
{
    internal Simulation Simulation;
    BufferPool MemoryPool;


    public static PhysicsWorld Create<TEngineNarrowPhase, TEnginePoseCallbacks>(TEngineNarrowPhase narrowPhase, TEnginePoseCallbacks poseIntegrator) where TEngineNarrowPhase : struct, IEngineNarrowPhase where TEnginePoseCallbacks : struct, IEnginePoseIntegrator
    {
        PhysicsWorld world = new PhysicsWorld();
        world.MemoryPool = new BufferPool();
        world.Simulation = Simulation.Create(world.MemoryPool, narrowPhase, poseIntegrator, new SolveDescription(8, 1));
        return world;

    }

    public void RegisterBody(bool isStatic, PhysicsBody body)
    {
        if (body.GetBody(Simulation, out BodyDescription desc))
        {
            body.handle = Simulation.Bodies.Add(desc);   
        }
    }

    protected PhysicsWorld()
    {
        
    }

    /// <summary>
    /// Runs on the level in a fixed time-step.
    /// </summary>
    /// <param name="delta"></param>
    public void Simulate(float delta)
    {
        PreSimulate(delta);
        // Engine simulation loop here. 
        Simulation.Timestep(delta);
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