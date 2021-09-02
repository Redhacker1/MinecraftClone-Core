using System.Collections.Generic;
using Engine.Physics;

namespace Engine.Objects
{
    public class Level : GameObject
    {
        List<GameObject> Objects = new List<GameObject>();

        public virtual List<Aabb> GetAabbs(int collisionlayer, Aabb collision)
        {
            return new List<Aabb>();
        }
    }
}