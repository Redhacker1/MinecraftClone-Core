using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Physics;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace Engine.Objects
{
    using Vector3 = Vector3;



    /// <summary>
    /// Used for entities that move around. This is based off of Entity
    /// </summary>
    public abstract class CharacterEntity : Entity
    {
        public bool OnGround;
        public double EyeOffset = 1.6f;
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
            

            OnGround = Math.Abs(o.Y - acceleration.Y) > double.Epsilon && o.Y < 0;

            if (Math.Abs(o.X - acceleration.X) > float.Epsilon) PosDelta.X = 0;
            if (Math.Abs(o.Y - acceleration.Y) > float.Epsilon) PosDelta.Y = 0;
            if (Math.Abs(o.Z - acceleration.Z) > float.Epsilon) PosDelta.Z = 0;

            Pos.X = (Aabb.MinLoc.X + Aabb.MaxLoc.X) / 2.0f;
            Pos.Y = (Aabb.MinLoc.Y + EyeOffset);
            Pos.Z = (Aabb.MinLoc.Z + Aabb.MaxLoc.Z) / 2.0f;
            
        }

        public void SetLevel(Level desiredLevel)
        {
            Level = desiredLevel;
        }

        public CharacterEntity(Vector3 position, Vector2 rotation, Level level)
        {
            
            Pos = position;
            double w = AabbWidth / 2.0;
            double h = AabbHeight / 2.0;
            
            Aabb = new Aabb(new Vector3((Pos.X - w), (Pos.Y - h), (Pos.Z - w)), new Vector3((Pos.X + w), (Pos.Y + h), (Pos.Z + w)));
            
            Level = level;
        }
        
        public CharacterEntity()
        {
            
            double w = AabbWidth / 2.0;
            double h = AabbHeight / 2.0;
            
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
            double w = AabbWidth / 2.0;
            double h = AabbHeight / 2.0;
            
            Aabb.MinLoc = new Vector3((pos.X - w), (pos.Y - h), (pos.Z - w));
            Aabb.MaxLoc = new Vector3((pos.X + w), (pos.Y + h), (pos.Z + w));
        }

        public virtual void Rotate(double rotX, double rotY)
        {
            Rotation.X = (Rotation.Y - rotX * 0.15);
            Rotation.Y = ((Rotation.Y + rotY * 0.15) % 360.0);
            
            if (Rotation.X < -90.0) Rotation.X = -90.0;
            if (Rotation.X > 90.0) Rotation.X = 90.0;
        }
    }
}
