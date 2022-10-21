using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Engine.Collision.Simple;

public struct AABB
{
    
    internal Vector128<float> _origin;
    public Vector3 Origin
    {
        get => _origin.AsVector3();
        set => _origin = value.AsVector128();
    }

    internal Vector128<float> _extents;
    public Vector3 Extents     
    {
        get => _extents.AsVector3();
        set => _extents = value.AsVector128();
    }
    
    public AABB(Vector3 origin, Vector3 extents)
    {
        _origin = default;
        _extents = default;
        
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