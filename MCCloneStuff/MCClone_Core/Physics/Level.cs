using System.Collections.Generic;
using Engine.Objects;

namespace MCClone_Core.Physics
{
    public class Level : GameObject
    {
        List<GameObject> _objects = new List<GameObject>();

        public virtual List<Aabb> GetAabbs(int collisionlayer, Aabb collision)
        {
            return new List<Aabb>();
        }
    }
}