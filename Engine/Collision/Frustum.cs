using System;
using System.Numerics;

namespace Engine.Collision;

    public struct Frustum
    {
        internal Plane[] Planes;
        public Vector3 camerapos;

        public Frustum(float FOV,float near, float far,float AspectRatio, Matrix4x4 ViewFrustum, Vector3 Pos, Plane[] planes)
        {

            if (planes.Length < 6)
            {
                throw new ArgumentException("the planes array was too small");
            }
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
            

            // Near
            planes[0] = new Plane(nearTopRight, nearTopLeft, nearBottomLeft);
            // Far
            planes[1] = new Plane(farTopLeft, farTopRight, farBottomRight);
            // Left
            planes[2] = new Plane(nearTopLeft, farTopLeft, nearBottomLeft);
            // Right
            planes[3] = new Plane(farTopRight, nearTopRight, farBottomRight);
            // Top
            planes[4] = new Plane(nearTopLeft, nearTopRight, farTopLeft);
            //Bottom
            planes[5] = new Plane(nearBottomLeft, farBottomLeft, farBottomRight);
            Planes =  planes;
        }
    }