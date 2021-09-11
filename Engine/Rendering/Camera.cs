using System;
using System.Numerics;
using Engine.MathLib;

namespace Engine.Rendering
{
    // TODO: More elegant Camera class, preferably one that can more easily accommodate more than one camera. 
    public class Camera
    {
        public static Camera MainCamera;

        public Engine.MathLib.DoublePrecision_Numerics.Vector3 Position { get; set; }
        public Vector3 Front { get; set; }

        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Up, Front));
        public Vector3 Up { get; private set; }
        public float AspectRatio { get; set; }

        public float Yaw { get; set; } = -90f;
        public float Pitch { get; set; }

        private float Zoom = 45f;

        public float NearPlane = .1f;
        public float FarPlane = 1000;

        public Camera(Vector3 position, Vector3 front, Vector3 up, float aspectRatio, bool mainCamera)
        {
            Position = position;
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

        public void ModifyDirection(float xOffset, float yOffset)
        {
            Yaw += xOffset;
            Pitch -= yOffset;

            //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            Pitch = Math.Clamp(Pitch, -89f, 89f);

            Vector3 cameraDirection = Vector3.Zero;
            cameraDirection.X = (float)Math.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            cameraDirection.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
            cameraDirection.Z = (float)Math.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));

            Front = Vector3.Normalize(cameraDirection);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Position, (Vector3)Position + Front, Up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), AspectRatio, NearPlane, FarPlane);
        }

        public float GetFOV()
        {
            return MathHelper.DegreesToRadians(90);
        }
    }
}