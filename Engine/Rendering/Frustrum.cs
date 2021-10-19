using System;
using System.Numerics;
using Engine.Renderable;

namespace Engine.Rendering
{

    public struct AABB // Alternative: aabb_t { float3 min; float3 max; };
    {
        public Vector3 center;
        public Vector3 extents;
        public Vector3 Max;
        public Vector3 Min;

        public AABB(Vector3 min, Vector3 max)
        {
            center = (min + max) * .5f;
            extents = max - center;
            Max = max;
            Min = min;
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
        internal MathLib.DoublePrecision_Numerics.Vector3 camerapos;

        public Frustrum(float FOV,float near, float far,float AspectRatio, Matrix4x4 ViewFrustum, MathLib.DoublePrecision_Numerics.Vector3 Pos)
        {
            camerapos = Pos;
            Matrix4x4.Invert(ViewFrustum, out Matrix4x4 thingmat);

            
            Vector3 mat3 = new Vector3(thingmat.M41, thingmat.M42, thingmat.M43);
            Vector3 mat2 = new Vector3(thingmat.M31, thingmat.M32, thingmat.M33);
            Vector3 mat1 = new Vector3(thingmat.M21, thingmat.M22, thingmat.M23);
            Vector3 mat0 = new Vector3(thingmat.M11, thingmat.M12, thingmat.M13);
            
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

            Planes = new[]
            {
                //Near
                new Plane(nearTopRight, nearTopLeft, nearBottomLeft),

                //Far
                new Plane(farTopLeft,farTopRight, farBottomRight),

                //Left
                new Plane(nearTopLeft, farTopLeft, nearBottomLeft),

                //Right
                new Plane(farTopRight, nearTopRight, farBottomRight),

                //Top
                new Plane(nearTopLeft, nearTopRight, farTopLeft),
                //Bottom
                new Plane(nearBottomLeft , farBottomLeft, farBottomRight),
            };
        }
    }

    public static class IntersectionHandler
    {
        static int SphereToPlane(ref Plane plane, ref Sphere sphere)
        {
            float distance = Vector3.Dot(sphere.Position, plane.Normal) - plane.Offset;
            if (distance > sphere.Radius)
            {
                return 1;
            }
            if (distance < -sphere.Radius)
            {
                return -1;
            }
            return 0;
        }


        public static int AABBToPlane(ref Plane plane, ref AABB aabb)
        {   
            Vector3 absnormal = Vector3.Abs(new Vector3(plane.Normal.X, plane.Normal.Y, plane.Normal.Z));
            Sphere sphere = new(Vector3.Dot(absnormal, aabb.extents), aabb.center);
            return SphereToPlane(ref plane, ref sphere );
        }
        

        public static bool MeshInFrustrum(Mesh mesh, ref Frustrum? frustum)
        {

            if (frustum.HasValue)
            {
                AABB meshAabb = new(mesh.minpoint - (Vector3)(frustum.Value.camerapos) , mesh.maxpoint - (Vector3)(frustum.Value.camerapos));

                var Frustum = frustum.Value;
                return aabb_to_frustum(ref meshAabb, ref Frustum);   
            }
            else
            {
                return true;
            }
        }
        
        static bool aabb_to_frustum(ref AABB aabb, ref Frustrum frustum)
        {
            for (int i = 0; i < frustum.Planes.Length; ++i)
            {
                if (AABBToPlane( ref frustum.Planes[i],ref aabb ) == 1)
                {
                    return false;
                }
            }
            return true;
        }
    }
        
}