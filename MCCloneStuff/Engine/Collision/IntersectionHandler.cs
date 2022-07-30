using System;
using System.Numerics;
using Engine.Rendering.Abstract;

namespace Engine.Collision;

    public static class IntersectionHandler
    {
        private static int SphereToPlane(ref Plane plane, ref Sphere sphere)
        {
            float num = Vector3.Dot(sphere.Position, plane.Normal) - plane.Offset;
            if (num > sphere.Radius)
                return 1;
            return num < - sphere.Radius ? -1 : 0;
        }

        public static int AABBToPlane(ref Plane plane, ref AABB aabb)
        {

            Sphere sphere = new(Vector3.Dot(Vector3.Abs(new(plane.Normal.X, plane.Normal.Y, plane.Normal.Z)), aabb.Extents), aabb.Origin);
            return SphereToPlane(ref plane, ref sphere);
        }

        public static bool MeshInFrustrum(Instance3D mesh, Frustum frustum)
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
                if (AABBToPlane(ref frustum.Planes[index], ref aabb) == 1)
                    return false;
            } 
            return true;
        }
    }