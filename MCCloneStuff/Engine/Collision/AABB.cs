using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.Collision;

public struct AABB
{
    public Vector3 Origin;
    public Vector3 Extents { get; private set; }
    
    public AABB(Vector3 origin, Vector3 extents)
    {
        Origin = origin;
        Extents = extents;
    }

    public void SetExtents(Vector3 extents)
    {
        Extents = extents;
    }

    public void SetAABB(Vector3 min, Vector3 max)
    {
        Origin = (min + max) * .5f;
        Extents = max - Origin;
    }

    /// <summary>
    /// Returns min and max values for each entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetMinMax( out Vector3 Min, out Vector3 Max)
    {
        Min = Origin - Extents;
        Max = Origin + Extents;
    }
}