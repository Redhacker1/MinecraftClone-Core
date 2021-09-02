using Engine.MathLib.DoublePrecision_Numerics;

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
        public Vector3 Rotation = Vector3.Zero;

    }
}