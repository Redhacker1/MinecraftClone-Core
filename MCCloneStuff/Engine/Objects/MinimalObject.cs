

using System.Numerics;

namespace Engine.Objects
{   /// <summary>
    /// Has a position, Location and rotation, cannot directly interact with the world. 
    /// </summary>
    public class MinimalObject
    {
        /// <summary>
        /// The world position of where the object is.
        /// </summary>
        public Vector3 Pos = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;

    }
}