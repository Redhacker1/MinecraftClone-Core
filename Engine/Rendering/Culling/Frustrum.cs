using System;
using System.Numerics;
using Engine.Renderable;

namespace Engine.Rendering.Shared.Culling
{

    public struct Aabb // Alternative: aabb_t { float3 min; float3 max; };
    {
        public Vector3 Center;
        public Vector3 Extents;
        public Vector3 Max;
        public Vector3 Min;

        public Aabb(Vector3 min, Vector3 max)
        {
            Center = (min + max) * .5f;
            Extents = max - Center;
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
        public Sphere(float radius, Vector3 position)
        {
            Radius = radius;
            this.Position = position;
        }
    }
    
    public struct Frustrum
    {
        internal Plane[] Planes;
        internal MathLib.DoublePrecision_Numerics.Vector3 Camerapos;

        public Frustrum(float fov,float near, float far,float aspectRatio, Matrix4x4 viewFrustum, MathLib.DoublePrecision_Numerics.Vector3 pos)
        {
            
            
            Camerapos = pos;
            Matrix4x4.Invert(viewFrustum, out Matrix4x4 thingmat);

            
            Vector3 mat3 = new Vector3(thingmat.M41, thingmat.M42, thingmat.M43);
            Vector3 mat2 = new Vector3(thingmat.M31, thingmat.M32, thingmat.M33);
            Vector3 mat1 = new Vector3(thingmat.M21, thingmat.M22, thingmat.M23);
            Vector3 mat0 = new Vector3(thingmat.M11, thingmat.M12, thingmat.M13);
            
            Vector3 nearCenter = mat3 - mat2 * near;
            Vector3 farCenter = mat3 - mat2 * far;
            
            float nearHeight = MathF.Tan(fov/2)* 2 * near;
            float farHeight = MathF.Tan(fov/2) * 2 * far;
            
            float nearWidth = nearHeight * aspectRatio;
            float farWidth = farHeight * aspectRatio;

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

        static Vector3 op_Subtraction(MathLib.DoublePrecision_Numerics.Vector3 first , Vector3 second)
        {
            return new Vector3((float)(first.X - second.X), (float)(first.Y - second.Y), (float)(first.Z - second.Z));
        }
    }

    public static class IntersectionHandler
    {
        public static int SphereToPlane(ref Plane plane, ref Sphere sphere)
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


        public static int AabbToPlane(ref Plane plane, ref Aabb aabb)
        {
            Vector3 absnormal = new Vector3(Math.Abs(plane.Normal.X), Math.Abs(plane.Normal.Y), Math.Abs(plane.Normal.Z));
            Sphere sphere = new Sphere(Vector3.Dot(absnormal, aabb.Extents), aabb.Center);
            return SphereToPlane(ref plane, ref sphere );
        }
        

        public static bool MeshInFrustrum(Mesh mesh, Frustrum? frustum, bool frustumculling = true)
        {

            if (frustum != null && mesh != null)
            {

                Frustrum Frustum = frustum.Value;
                return aabb_to_frustum(new Aabb(mesh.Minpoint - (Vector3)Camera.MainCamera.Pos, mesh.Maxpoint - (Vector3)Camera.MainCamera.Pos), ref Frustum);   
            }

            return true;
        }
        
        internal static bool aabb_to_frustum(Aabb aabb, ref Frustrum frustum)
        {
            for (int i = 0; i < frustum.Planes.Length; ++i)
            {
                if (AabbToPlane( ref frustum.Planes[i],ref aabb ) == 1)
                {
                    return false;
                }
            }
            return true;
        }
    }
        
}