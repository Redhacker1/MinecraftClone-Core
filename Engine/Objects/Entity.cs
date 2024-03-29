﻿using System.Numerics;
using Engine.Collision.Simple;
using Engine.MathLib;

namespace Engine.Objects
{
    /// <summary>
    /// This is used primarily for static entities, this class has less movement, have an AABB (if you want them to),
    /// a position, rotation and are able to interact with the world like a game object,
    /// if they are purely for running a script and dont require any of that, use a GameObject instead
    /// </summary>
    public class Entity : EngineObject
    {
        public AABB bbox { get; set; }

        public Vector3 PosDelta;


        public Entity(Transform transform, EngineObject parent = null) : base(transform, parent)
        {
            
        }

        protected Entity()
        {
            
        }
    }
}