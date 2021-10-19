using System.Numerics;
using Engine.Physics;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace Engine.Objects
{
    /// <summary>
    /// This is used primarily for static entities, this class has less movement have an AABB (if you want them to),
    /// a position, rotation and are able to interact with the world like a game object,
    /// if they are purely for running a script and dont require any of that, use a GameObject instead
    /// </summary>
    public class Entity : GameObject
    {
        public double AABBWidth = .9f;
        public double AABBHeight = 1.95f;
        
        public Vector3 PosDelta;
        public Aabb AABB;

        public Entity(Vector3 pos, Vector2 rotation)
        {
            Pos = pos;
            
            double w = AABBWidth / 2.0f;
            double h = AABBHeight / 2.0f;

            AABB = new Aabb(new Vector3((pos.X - w), (pos.Y - h), (pos.Z - w)), new Vector3((pos.X + w), (pos.Y + h), (pos.Z + w)));
            
            Rotation.X = (Rotation.X - rotation.X * 0.15);
            Rotation.Y = ((Rotation.Y + rotation.Y * 0.15) % 360.0);
        }

        protected Entity()
        {
            
        }
    }
}