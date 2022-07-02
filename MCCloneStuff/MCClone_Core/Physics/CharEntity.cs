using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Objects;

namespace MCClone_Core.Physics
{
    /// <summary>
    /// Used for entities that move around. This is based off of Entity
    /// </summary>
    public abstract class CharacterEntity : Entity
    {
        public bool OnGround;
        public float EyeOffset = 1.6f;
        public bool Noclip = true;

        public Level Level;

        public virtual void Move(Vector3 accel)
        {

            Vector3 acceleration = accel;

            Vector3 o = acceleration;
            
            
            List<Aabb> aabbs = Level?.GetAabbs(0, Aabb.Expand(acceleration));
            if (Noclip)
            {
                aabbs = new List<Aabb>();
            }
            

            foreach (Aabb aabb in aabbs)
            {
                acceleration.Y = aabb.ClipYCollide(Aabb, acceleration.Y);
            }
            Aabb.Move(new Vector3(0, acceleration.Y, 0));
            foreach (Aabb aabb in aabbs)
            {
                acceleration.X = aabb.ClipXCollide(Aabb, acceleration.X);
            }
            Aabb.Move(new Vector3(acceleration.X, 0, 0));
            foreach (Aabb aabb in aabbs)
            {
                acceleration.Z = aabb.ClipZCollide(Aabb, acceleration.Z);
            }
            Aabb.Move(new Vector3(0, 0, acceleration.Z));
            

            OnGround = Math.Abs(o.Y - acceleration.Y) > 0.001f && o.Y < 0;

            if (Math.Abs(o.X - acceleration.X) > 0.001f) PosDelta.X = 0;
            if (Math.Abs(o.Y - acceleration.Y) > 0.001f) PosDelta.Y = 0;
            if (Math.Abs(o.Z - acceleration.Z) > 0.001f) PosDelta.Z = 0;

            Pos.X = (Aabb.MinLoc.X + Aabb.MaxLoc.X) / 2.0f;
            Pos.Y = (Aabb.MinLoc.Y + EyeOffset);
            Pos.Z = (Aabb.MinLoc.Z + Aabb.MaxLoc.Z) / 2.0f;
            
        }

        public void SetLevel(Level desiredLevel)
        {
            Level = desiredLevel;
        }
        
        public Aabb Aabb;
        public CharacterEntity(Vector3 position, Vector2 rotation, Level level)
        {
            PhysicsTick = true;
            Ticks = true;
            Pos = position;
            float w = AabbWidth / 2.0f;
            float h = AabbHeight / 2.0f;
            
            Aabb = new Aabb(new Vector3((Pos.X - w), (Pos.Y - h), (Pos.Z - w)), new Vector3((Pos.X + w), (Pos.Y + h), (Pos.Z + w)));
            
            Level = level;
        }
        
        public CharacterEntity()
        {
            PhysicsTick = true;
            Ticks = true;
            
            float w = AabbWidth / 2.0f;
            float h = AabbHeight / 2.0f;
            
            Aabb = new Aabb(new Vector3((Pos.X - w), (Pos.Y - h), (Pos.Z - w)), new Vector3((Pos.X + w), (Pos.Y + h), (Pos.Z + w)));
            
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
            Pos = pos;
            float w = AabbWidth / 2.0f;
            float h = AabbHeight / 2.0f;
            
            Aabb.MinLoc = new Vector3((pos.X - w), (pos.Y - h), (pos.Z - w));
            Aabb.MaxLoc = new Vector3((pos.X + w), (pos.Y + h), (pos.Z + w));
        }

        public virtual void Rotate(float rotX, float rotY)
        {
            Rotation.X = (Rotation.Y - rotX * 0.15f);
            Rotation.Y = ((Rotation.Y + rotY * 0.15f) % 360.0f);
            
            if (Rotation.X < -90.0) Rotation.X = -90.0f;
            if (Rotation.X > 90.0) Rotation.X = 90.0f;
        }
    }
}
