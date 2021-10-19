using System;
using System.Collections.Generic;
using Engine.Physics;

namespace Engine.Objects
{
    public class Level : GameObject
    {
        List<WeakReference<GameObject>> Objects = new List<WeakReference<GameObject>>();

        public virtual List<Aabb> GetAabbs(int collisionlayer, Aabb collision)
        {
            return new List<Aabb>();
        }
    }
}