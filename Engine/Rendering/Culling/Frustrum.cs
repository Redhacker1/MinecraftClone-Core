using System;
using System.Numerics;
using Engine.Renderable;

namespace Engine.Rendering.Culling
{
    struct OBB
    {
        Vector3 center;
        Vector3 extents;
        // Orthonormal basis
        Quaternion Rotation;
}

    public struct AABB // Alternative: aabb_t { float3 min; float3 max; };
    {
        public Vector3 center;
        public Vector3 extents;

        public AABB(Vector3 min, Vector3 max)
        {
            center = (min + max) * .5f;
            extents = max - center;
        }
    }
    public struct Plane
    {
        public Vector3 Normal;
        public float Offset;
        public Plane(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Normal = Vector3.Normalize(Vector3.Cross(p1-p0, p2 - p1));
            Offset = Vector3.Dot(Normal, p0);
        }
    }

    public struct Sphere
    {
        public float Radius;
        public Vector3 Position;
        public Sphere(float radius, Vector3 Position)
        {
            Radius = radius;
            this.Position = Position;
        }
    }
    
    public struct Frustrum
    {
        static Mesh[] FrustrumMeshes;
        internal Plane[] Planes;
        public Vector3 camerapos;

        public Frustrum(float FOV,float near, float far,float AspectRatio, Matrix4x4 ViewFrustum, Vector3 Pos, ref Plane[] planes)
        {
            camerapos = Pos;
            Matrix4x4.Invert(ViewFrustum, out Matrix4x4 thingmat);

            
            Vector3 mat3 = new(thingmat.M41, thingmat.M42, thingmat.M43);
            Vector3 mat2 = new(thingmat.M31, thingmat.M32, thingmat.M33);
            Vector3 mat1 = new(thingmat.M21, thingmat.M22, thingmat.M23);
            Vector3 mat0 = new(thingmat.M11, thingmat.M12, thingmat.M13);
            
            Vector3 nearCenter = mat3 - mat2 * near;
            Vector3 farCenter = mat3 - mat2 * far;
            
            float nearHeight = MathF.Tan(FOV/2)* 2 * near;
            float farHeight = MathF.Tan(FOV/2) * 2 * far;
            
            float nearWidth = nearHeight * AspectRatio;
            float farWidth = farHeight * AspectRatio;

            Vector3 farTopLeft = (farCenter + mat1 * (farHeight * 0.5f) - mat0 * (farWidth * 0.5f)) * 1f;
            Vector3 farTopRight = (farCenter + mat1 * (farHeight * 0.5f) + mat0 * (farWidth * 0.5f)) * 1f;
            Vector3 farBottomLeft = (farCenter - mat1 * (farHeight * 0.5f) - mat0 * (farWidth * 0.5f)) * 1f;
            Vector3 farBottomRight = (farCenter - mat1 * (farHeight * 0.5f) + mat0 * (farWidth * 0.5f)) * 1f;

            Vector3 nearTopLeft = (nearCenter + mat1 * (nearHeight * 0.5f) - mat0 * (nearWidth * 0.5f)) * 1f;
            Vector3 nearTopRight = (nearCenter + mat1 * (nearHeight * 0.5f) + mat0 * (nearWidth * 0.5f)) * 1f;
            Vector3 nearBottomLeft = (nearCenter - mat1 * (nearHeight * 0.5f) - mat0 * (nearWidth * 0.5f)) * 1f;
            

            // Near
            planes[0] = new(nearTopRight, nearTopLeft, nearBottomLeft);
            // Far
            planes[1] = new(farTopLeft, farTopRight, farBottomRight);
            // Left
            planes[2] = new(nearTopLeft, farTopLeft, nearBottomLeft);
            // Right
            planes[3] = new(farTopRight, nearTopRight, farBottomRight);
            // Top
            planes[4] = new(nearTopLeft, nearTopRight, farTopLeft);
            //Bottom
            planes[5] = new(nearBottomLeft, farBottomLeft, farBottomRight);
            Planes =  planes;
        }
    }

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

            Sphere sphere = new(Vector3.Dot(Vector3.Abs(new(plane.Normal.X, plane.Normal.Y, plane.Normal.Z)), aabb.extents), aabb.center);
            return SphereToPlane(ref plane, ref sphere);
        }

        public static bool MeshInFrustrum(Mesh mesh, Frustrum frustum)
        {
            if (mesh != null)
            {

                Span<Vector3> outValues = stackalloc Vector3[2];
                mesh?.GetMinMaxScaled(outValues, frustum.camerapos);
                AABB aabb = new(outValues[0], outValues[1]);
                return aabb_to_frustum(ref aabb, frustum);   
            }

            return true;
        }


        public static bool aabb_to_frustum(ref AABB aabb, Frustrum frustum)
        {

            for (int index = 0; index < frustum.Planes.Length; ++index)
            {
                if (AABBToPlane(ref frustum.Planes[index], ref aabb) == 1)
                    return false;
            } 
            return true;
        }
    }
        
}