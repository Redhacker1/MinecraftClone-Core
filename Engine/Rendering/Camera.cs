using System;
using System.Numerics;
using Engine.MathLib;
using Engine.Objects;

namespace Engine.Rendering
{
    // TODO: More elegant Camera class, preferably one that can more easily accommodate more than one camera. 
    public class Camera : MinimalObject
    {
        public static Camera MainCamera;
        public Vector3 Front { get; set; }

        public Vector3 Right
        {
            get => Vector3.Normalize(Vector3.Cross(Up, Front));
        }

        public Vector3 Up { get; private set; }
        public float AspectRatio { get; set; }

        public float Yaw { get; set; } = -90f;
        public float Pitch { get; set; }

        private float Zoom = 45f;

        public float NearPlane = .1f;
        public float FarPlane = 1000;

        public Camera(Vector3 pos, Vector3 front, Vector3 up, float aspectRatio, bool mainCamera)
        {
            Pos = pos;
            AspectRatio = aspectRatio;
            Front = front;
            Up = up;
            
            if (mainCamera)
            {
                MainCamera = this;
            }
        }

        public void ModifyZoom(float zoomAmount)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            Zoom = Math.Clamp(Zoom - zoomAmount, 1.0f, 45f);
        }

        internal Frustrum GetViewFrustum()
        {
            Frustrum frustum = new Frustrum(GetFOV(), NearPlane, FarPlane, AspectRatio, GetViewMatrix(), Pos);

            return frustum;
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Vector3.Zero, Front, Up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(GetFOV(), AspectRatio, NearPlane, FarPlane);
        }

        public float GetFOV()
        {
            return MathHelper.DegreesToRadians(110);
        }
    }
}