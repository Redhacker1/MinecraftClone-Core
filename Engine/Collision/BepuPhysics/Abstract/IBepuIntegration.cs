using BepuPhysics;

namespace Engine.Collision.BEPUPhysics;

public interface IBepuIntegration
{
    public Simulation GetSimulation { get; protected internal set; }

}