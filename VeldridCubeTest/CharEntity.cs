using System.Numerics;
using Engine.MathLib;
using Engine.Objects;

namespace VeldridCubeTest
{
    /// <summary>
    /// Used for entities that move around. This is based off of Entity
    /// </summary>
    public abstract class CharacterEntity : Entity
    {
        public bool OnGround;
        public float EyeOffset = 1.6f;
        public bool Noclip = true;
        

        public virtual void Move(Vector3 accel)
        {

            Vector3 acceleration = accel;

            Vector3 o = acceleration;
            
            
            Aabb.Move(acceleration);


            OnGround = Math.Abs(o.Y - acceleration.Y) > 0.001f && o.Y < 0;

            if (Math.Abs(o.X - acceleration.X) > 0.001f) PosDelta.X = 0;
            if (Math.Abs(o.Y - acceleration.Y) > 0.001f) PosDelta.Y = 0;
            if (Math.Abs(o.Z - acceleration.Z) > 0.001f) PosDelta.Z = 0;
            Vector3 pos = new Vector3()
            {
                X = (Aabb.MinLoc.X + Aabb.MaxLoc.X) / 2.0f,
                Y = Aabb.MinLoc.Y + EyeOffset,
                Z = (Aabb.MinLoc.Z + Aabb.MaxLoc.Z) / 2.0f
            };
            SetPos(pos);
            
        }
        
        
        public Aabb Aabb;
        public CharacterEntity(Vector3 position, Vector2 rotation)
        {
            PhysicsTick = true;
            Ticks = true;
            float w = AabbWidth / 2.0f;
            float h = AabbHeight / 2.0f;
            
            Aabb = new Aabb(new Vector3(position.X - w, position.Y - h, position.Z - w), new Vector3(position.X + w, position.Y + h, position.Z + w));
            
        }
        
        public CharacterEntity()
        {
            PhysicsTick = true;
            Ticks = true;
            
            float w = AabbWidth / 2.0f;
            float h = AabbHeight / 2.0f;
            
            Aabb = new Aabb(new Vector3(Position.X - w, Position.Y - h, Position.Z - w), new Vector3(Position.X + w, Position.Y + h, Position.Z + w));
            
        }

        public virtual void MoveRelative(double dx, double dz, double speed)
        {
            double dist = dx * dx + dz * dz;
            if (dist < 0.01f) return;

            dist = speed / Math.Sqrt(dist);
            double sin = Math.Sin(MathHelper.DegreesToRadians(Rotation.Y));
            double cos = Math.Cos(MathHelper.DegreesToRadians(Rotation.Y));

            PosDelta.X += (float)((dx *= dist) * cos - (dz *= dist) * sin);
            PosDelta.Z += (float)(dz * cos + dx * sin);
        }

        public virtual void SetPos(Vector3 pos)
        {
            Position = pos;
            float w = AabbWidth / 2.0f;
            float h = AabbHeight / 2.0f;
            
            Aabb.MinLoc = new Vector3(pos.X - w, pos.Y - h, pos.Z - w);
            Aabb.MaxLoc = new Vector3(pos.X + w, pos.Y + h, pos.Z + w);
        }

        public virtual void Rotate(float rotX, float rotY)
        {
            Vector3 tempRot = new Vector3();
            tempRot.X = Rotation.Y - rotX * 0.15f;
            tempRot.Y = (Rotation.Y + rotY * 0.15f) % 360.0f;
            
            if (Rotation.X < -90.0) tempRot.X = -90.0f;
            if (Rotation.X > 90.0) tempRot.X = 90.0f;

            Rotation = Quaternion.CreateFromYawPitchRoll(
                MathHelper.DegreesToRadians(tempRot.X),
                MathHelper.DegreesToRadians(tempRot.Y), 
                MathHelper.DegreesToRadians(tempRot.Z)
                );
        }
    }
}
