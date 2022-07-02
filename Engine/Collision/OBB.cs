using System.Numerics;

namespace Engine.Collision;

// TODO: Either implement or remove.
struct OBB
{
    Vector3 center;
    Vector3 extents;
    // Orthonormal basis
    Quaternion Rotation;
}