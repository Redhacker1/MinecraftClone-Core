using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Renderable;
using Silk.NET.OpenGL;

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

        public Frustrum(Camera baseCamera)
        {
            System.Numerics.Vector3 nearCenter = op_Subtraction(baseCamera.Pos, baseCamera.Front) * baseCamera.NearPlane;
            System.Numerics.Vector3 farCenter = op_Subtraction(baseCamera.Pos, baseCamera.Front) * baseCamera.FarPlane;

            float nearHeight = (float)(Math.Tan(baseCamera.GetFOV() / 2) * baseCamera.NearPlane);
            float farHeight = (float)(Math.Tan(baseCamera.GetFOV() / 2) * baseCamera.FarPlane);
            float nearWidth = nearHeight * baseCamera.AspectRatio;
            float farWidth = farHeight * baseCamera.AspectRatio;

            Vector3 farTopLeft = farCenter + baseCamera.Up * (farHeight * 0.5f) - baseCamera.Right * (farWidth * 0.5f);
            Vector3 farTopRight = farCenter + baseCamera.Up * (farHeight * 0.5f) + baseCamera.Right * (farWidth * 0.5f);
            Vector3 farBottomLeft = farCenter - baseCamera.Up * (farHeight * 0.5f) - baseCamera.Right * (farWidth * 0.5f);
            Vector3 farBottomRight = farCenter - baseCamera.Up * (farHeight * 0.5f) + baseCamera.Right * (farWidth * 0.5f);

            Vector3 nearTopLeft = new Vector3(-nearWidth, nearHeight, 1); //nearCenter + baseCamera.Up * (nearHeight * 0.5f) - baseCamera.Right * (nearWidth * 0.5f);
            Vector3 nearTopRight = new Vector3(nearWidth, nearHeight, 1);//nearCenter + baseCamera.Up * (nearHeight * 0.5f) + baseCamera.Right * (nearWidth * 0.5f);
            Vector3 nearBottomLeft = new Vector3(-nearWidth, -nearHeight, 1);//nearCenter - baseCamera.Up * (nearHeight * 0.5f) - baseCamera.Right * (nearWidth * 0.5f);
            Vector3 nearBottomRight = new Vector3(nearWidth, nearHeight, 1);//nearCenter - baseCamera.Up * (nearHeight * 0.5f) + baseCamera.Right * (nearWidth * 0.5f);

            Planes = new[]
            {
                //Near
                new Plane(nearBottomLeft, nearTopRight, nearTopLeft),

                //Far
                new Plane(farTopLeft,farTopRight, farBottomLeft),

                //Left
                new Plane(nearBottomLeft, farTopLeft, nearTopLeft ),

                //Right
                new Plane(nearBottomRight, farTopRight, nearTopRight),

                //Top
                new Plane(farTopLeft, nearTopRight, nearTopLeft),
                //Bottom
                new Plane(farBottomRight , farBottomLeft, nearBottomLeft),
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


        public static int AABBToPlane(ref Plane plane, ref AABB aabb)
        {
            Vector3 absnormal = new Vector3(Math.Abs(plane.Normal.X), Math.Abs(plane.Normal.Y), Math.Abs(plane.Normal.Z));
            Sphere sphere = new Sphere(Vector3.Dot(absnormal, aabb.extents), aabb.center);
            return SphereToPlane(ref plane, ref sphere );
        }
        
        static int TestAABBPlane(ref AABB b, ref Plane p) {
            // Convert AABB to center-extents representation
            Vector3 c = (b.Max + b.Min) * 0.5f; // Compute AABB center
            Vector3 e = b.Max - c; // Compute positive extents

            // Compute the projection interval radius of b onto L(t) = b.c + t * p.n
            float r = e.X * Math.Abs(p.Normal.X) + e.Y * Math.Abs(p.Normal.Y) + e.Z * Math.Abs(p.Normal.Z);

            // Compute distance of box center from plane
            float s = Vector3.Dot(p.Normal, c) - p.Offset;

            // Intersection occurs when distance s falls within [-r,+r] interval
            if (s > r)
            {
                return 1;
            }
            if (s < -r)
            {
                return -1;
            }
            return 0;
        }

        public static bool MeshInFrustrum(Mesh mesh, Camera camera)
        {
            var frustrum = new Frustrum(camera);
            var meshAABB = new AABB(mesh.minpoint, mesh.maxpoint);
            
            
            return aabb_to_frustum(ref meshAABB, ref frustrum);
        }
        
        static bool aabb_to_frustum(ref AABB aabb, ref Frustrum frustum)
        {
            for (int i = 0; i < frustum.Planes.Length; ++i)
            {
                if (TestAABBPlane(ref aabb, ref frustum.Planes[i] ) == -1)
                {
                    return false;
                }
            }
            return true;
        }
    }
        
}