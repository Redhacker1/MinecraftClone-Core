using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Engine.Rendering.Abstract;

namespace Engine.Collision.Simple;

    public static class IntersectionHandler
    {
        private static int SphereToPlane(Plane plane, Sphere sphere)
        {
            
            
            float num = Vector3.Dot(sphere.Position, plane.Normal) - plane.Offset;
            if (num > sphere.Radius)
                return 1;
            return num < - sphere.Radius ? -1 : 0;
        }

        

        static Vector128<float> Abs(Vector128<float> vector128)
        {
            return Sse.Max(Sse.Subtract(Vector128<float>.Zero, vector128), vector128);
        }

        public static int AabbToPlane(ref Plane plane, ref AABB aabb)
        {
            Vector3 abs = Vector3.Abs(plane.Normal);

            float res = Vector3.Dot(abs, aabb.Extents);

            Sphere sphere = new Sphere(res, aabb.Origin);
            return SphereToPlane(plane, sphere);
        }

        public static bool MeshInFrustum(Instance3D mesh, Frustum frustum)
        {
            if (mesh != null)
            {
                mesh.GetInstanceAabb(out AABB aabb , frustum.camerapos);
                return aabb_to_frustum(ref aabb, frustum);   
            }

            return true;
        }


        public static bool aabb_to_frustum(ref AABB aabb, Frustum frustum)
        {

            for (int index = 0; index < frustum.Planes.Length; ++index)
            {
                if (AabbToPlane(ref frustum.Planes[index], ref aabb) == 1)
                    return false;
            } 
            return true;
        }
    }