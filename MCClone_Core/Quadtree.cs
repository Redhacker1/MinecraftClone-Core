using System.Collections.Generic;
using System.Numerics;
using Engine.Collision.Simple;
using Silk.NET.Maths;

namespace MCClone_Core;

public class Quadtree : ISpacialPartitioningScheme
{
    Quadtree[] Leaves;
    List<ChunkBucket> Buckets = new List<ChunkBucket>();
    
    bool HasChildren => Leaves != null;
    readonly AABB _boundingBox;

    readonly int _limit;
    readonly int _depth;
    readonly int _splitSize;
    


    public Quadtree(Vector2 size, int RecursionLimit, int SplitSize)
    {
        _boundingBox = new AABB(Vector3.Zero, new Vector3(size.X, 0, size.Y));
        _limit = RecursionLimit;
        _splitSize = SplitSize;
        _depth = 0;
    }
    
    Quadtree(Quadtree parent, AABB boundingBox)
    {

        this._boundingBox = boundingBox;
        _limit = parent._limit;
        _splitSize = parent._splitSize;
        _depth = parent._depth + 1;
    }

    public void Add(ChunkBucket bucket)
    {
        // If it don't belong then dont add it
        if (IntersectionHandler.AABB_to_AABB(bucket.GetAABB(), GetAABB()))
        {
            //
            if (_splitSize >= Buckets.Count && _depth <= _limit && !HasChildren)
            {
                // Split and move all the objects into the next depth, if the objects overlap two or more cells, then keep the object here
                Recurse();
                return;
            }
            // Just add it
        }
    }
    

    void TestAABB(AABB testBox)
    {
        IntersectionHandler.AABB_to_AABB(_boundingBox, GetAABB());
    }

    void Recurse()
    {
        // Divide each element in half, to get a quarter of the area.
        Vector2 newSize = new Vector2(_boundingBox.Extents.X, _boundingBox.Extents.Z);
        Leaves = new Quadtree[4];

        AABB boundingBox = new AABB();
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                //boundingBox.SetAABB(new Vector2(x, y) * newSize, boundingBox.Origin);
                //Leaves[x + y] = new Quadtree(this, newSize * new Vector2(x + 1, y + 1));
            }
        }

        for (int i = 0; i < 1; i++)
        {
            
        }
        
    }

    public AABB GetAABB()
    {
        throw new System.NotImplementedException();
    }
    
    
}


static class SpacialWorldRepresentation
{
    
}

public class ChunkBucket : ISpacialPartitioningScheme
{
    Vector2 Location;
    public static readonly Vector2D<int> SizeMultiplier = new Vector2D<int>(4, 4);

    public AABB GetAABB()
    {
        throw new System.NotImplementedException();
    }
}

interface ISpacialPartitioningScheme
{
    AABB GetAABB();
}