using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Physics;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace Engine.Objects
{
    /// <summary>
    /// Used for entities that move around. This is based off of Entity
    /// </summary>
    public abstract class CharacterEntity : Entity
    {
        public bool OnGround;
        public double EyeOffset = 1.6f;
        public bool Noclip = true;

        public Level Level;

        public virtual void Move(Vector3 a)
        {

            Vector3 _a = a;

            Vector3 o = _a;
            
            
            List<Aabb> aabbs = Level?.GetAabbs(0, AABB.Expand(_a));
            if (Noclip)
            {
                aabbs = new List<Aabb>();
            }
            

            foreach (Aabb aabb in aabbs)
            {
                _a.Y = aabb.ClipYCollide(AABB, _a.Y);
            }
            AABB.Move(new Vector3(0, _a.Y, 0));
            foreach (Aabb aabb in aabbs)
            {
                _a.X = aabb.ClipXCollide(AABB, _a.X);
            }
            AABB.Move(new Vector3(_a.X, 0, 0));
            foreach (Aabb aabb in aabbs)
            {
                _a.Z = aabb.ClipZCollide(AABB, _a.Z);
            }
            AABB.Move(new Vector3(0, 0, _a.Z));
            

            OnGround = Math.Abs(o.Y - _a.Y) > double.Epsilon && o.Y < 0;

            if (Math.Abs(o.X - _a.X) > float.Epsilon) PosDelta.X = 0;
            if (Math.Abs(o.Y - _a.Y) > float.Epsilon) PosDelta.Y = 0;
            if (Math.Abs(o.Z - _a.Z) > float.Epsilon) PosDelta.Z = 0;

            Pos.X = (AABB.MinLoc.X + AABB.MaxLoc.X) / 2.0f;
            Pos.Y = (AABB.MinLoc.Y + EyeOffset);
            Pos.Z = (AABB.MinLoc.Z + AABB.MaxLoc.Z) / 2.0f;
            
        }

        public void SetLevel(Level desiredLevel)
        {
            Level = desiredLevel;
        }

        public CharacterEntity(Vector3 position, Vector2 rotation, Level level)
        {
            
            Pos = position;
            double w = AABBWidth / 2.0;
            double h = AABBHeight / 2.0;
            
            AABB = new Aabb(new Vector3((Pos.X - w), (Pos.Y - h), (Pos.Z - w)), new Vector3((Pos.X + w), (Pos.Y + h), (Pos.Z + w)));
            
            Level = level;
        }
        
        public CharacterEntity()
        {
            
            double w = AABBWidth / 2.0;
            double h = AABBHeight / 2.0;
            
            AABB = new Aabb(new Vector3((Pos.X - w), (Pos.Y - h), (Pos.Z - w)), new Vector3((Pos.X + w), (Pos.Y + h), (Pos.Z + w)));
            
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
            double w = AABBWidth / 2.0;
            double h = AABBHeight / 2.0;
            
            AABB.MinLoc = new Vector3((pos.X - w), (pos.Y - h), (pos.Z - w));
            AABB.MaxLoc = new Vector3((pos.X + w), (pos.Y + h), (pos.Z + w));
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
