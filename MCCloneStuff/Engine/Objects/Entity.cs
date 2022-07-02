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


        public Entity(Vector3 pos, Vector2 rotation)
        {
            Pos = pos;
            Rotation.X = (Rotation.X - rotation.X * 0.15f);
            Rotation.Y = ((Rotation.Y + rotation.Y * 0.15f) % 360.0f);
        }

        protected Entity()
        {
            
        }
    }
}