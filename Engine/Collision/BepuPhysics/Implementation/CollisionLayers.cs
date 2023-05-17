using System;

namespace Engine.Collision.BEPUPhysics.Implementation;

/// <summary>
/// TODO: Implement a more flexable collison system
/// </summary>
[Flags]
public enum CollisionLayers : long
{
    /// <summary>
    /// Can collide no matter what we hit!
    /// </summary>
    Everything  = 0b1,
    // Players, Bots, other Characters like that
    NPC = 0b10,
    // Triggers
    Triggers = 0b100,
    // Not Implemented
    Layer1 = 0b1000,
    // Not Implemented
    Layer2 = 0b1000,
    // Not Implemented
    Layer3 = 0b1000,
    // Not Implemented
    Layer4 = 0b1000,


}