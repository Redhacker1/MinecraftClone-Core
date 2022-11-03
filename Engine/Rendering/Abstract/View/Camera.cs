using System;
using System.Numerics;
using Engine.Collision.Simple;
using Engine.MathLib;
using Engine.Objects;
using Plane = Engine.Collision.Simple.Plane;

namespace Engine.Rendering.Abstract
{
    // TODO: More elegant Camera class, preferably one that can more easily accommodate more than one camera. 
    public class Camera : EngineObject
    {
        public static Camera MainCamera;
        public Vector3 Front { get; set; }
        public float FOV = 90;
        Plane[] frustumPlanes = new Plane[6];
        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Up, Front));
        public Vector3 Up { get; private set; }
        public float AspectRatio { get; set; }

        public float Yaw { get; set; } = 0;
        public float Pitch { get; set; }

        float _zoom = 45f;

        public float NearPlane = .1f;
        public float FarPlane = 1000;

        public Vector3 Forward => GetLookDir();

        public Camera(Transform transform, Vector3 front, Vector3 up, float aspectRatio, bool mainCamera)
        {
            LocalTransform = transform;
            AspectRatio = aspectRatio;
            Front = front;
            Up = up;
            
            if (mainCamera)
            {
                MainCamera = this;
            }
        }
        

        Vector3 GetLookDir()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            return lookDir;
        }

        public void ModifyZoom(float zoomAmount)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            _zoom = Math.Clamp(_zoom - zoomAmount, 1.0f, 45f);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Vector3.Zero, Front, Vector3.UnitY);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(GetFov(), AspectRatio, NearPlane, FarPlane);
        }

        public Frustum GetViewFrustum(out Plane[] planes)
        {
            Frustum frustum = new Frustum(GetFov(), NearPlane, FarPlane, AspectRatio, GetViewMatrix(), LocalTransform.Position, frustumPlanes);
            planes = frustumPlanes;
            return frustum;
        }

        public float GetFov()
        {
            return MathHelper.DegreesToRadians(FOV);
        }
    }
}