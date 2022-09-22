using System;

namespace Engine.Objects
{
    /// <summary>
    /// Base class for anything that interacts with the world, has a Free() method like godot, but internally implements IDisposable to allow for both using statements and and to keep an accurate tally from what remains, unlike godot, this can be Garbage collected safely, so entities with no references will eventually be removed
    /// </summary>
    [Obsolete("Functionality moved to EngineObject")]
    public class GameObject : EngineObject
    {
    }
}