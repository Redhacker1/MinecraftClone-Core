using System.Numerics;

namespace Engine.Objects
{
    /// <summary>
    /// This is used primarily for static entities, this class has less movement, have an AABB (if you want them to),
    /// a position, rotation and are able to interact with the world like a game object,
    /// if they are purely for running a script and dont require any of that, use a GameObject instead
    /// </summary>
    public class Entity : GameObject
    {
        public float AabbWidth = 0;
        public float AabbHeight = 0;
        
        public Vector3 PosDelta;


        public Entity(Vector3 pos, Quaternion rotation)
        {
            Position = pos;
        }

        protected Entity()
        {
            
        }
    }
}