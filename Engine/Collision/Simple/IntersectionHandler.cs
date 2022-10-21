using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Engine.Rendering.Abstract;

namespace Engine.Collision.Simple;

    public static class IntersectionHandler
    {
        private static int SphereToPlane(Plane plane, Sphere sphere)
        {
            
            
            float num = DotProductSimd(sphere.PosSimd, plane.SIMDnormal) - plane.Offset;
            if (num > sphere.Radius)
                return 1;
            return num < - sphere.Radius ? -1 : 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float DotProductSimd(Vector128<float> one, Vector128<float> two)
        {
            Vector128<float> mulRes = Sse.Multiply(one, two);
            Vector128<float> shufReg = Sse3.MoveHighAndDuplicate(mulRes);
            Vector128<float> sumsReg = Sse.Add(mulRes, shufReg);
            
            shufReg = Sse.MoveHighToLow(shufReg, sumsReg);
            sumsReg = Sse.Add(sumsReg, shufReg);
            return sumsReg.ToScalar();
        }

        static Vector128<float> Abs(Vector128<float> vector128)
        {
            return Sse.Max(Sse.Subtract(Vector128<float>.Zero, vector128), vector128);
        }

        public static int AabbToPlane(Plane plane, AABB aabb)
        {
            Vector128<float> abs = Abs(plane.SIMDnormal);

            float res = DotProductSimd(abs, aabb._extents);

            Sphere sphere = new Sphere(aabb._origin.WithElement(3, res));
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
                if (AabbToPlane(frustum.Planes[index], aabb) == 1)
                    return false;
            } 
            return true;
        }
    }