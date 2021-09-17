using System;
using System.Numerics;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering;

namespace CullingTests
{
    public class Player : CharacterEntity
    {
        Camera cam;
        DemoController Controller;
        protected override void _Ready()
        {
            base._Ready();
            Controller = new DemoController(this);
            cam = new Camera(Pos, -Vector3.UnitZ, Vector3.UnitY, 1.777778F, true);
        }
        
        public Player()
        {
            
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            
        }
        
        
        public void Freelook()
        {
            float xOffset = InputHandler.MouseDelta(0).X * 0.1f;
            float yOffset = InputHandler.MouseDelta(0).Y * 0.1f;

            if (cam != null)
            {
                Camera.MainCamera.Yaw += xOffset;
                Camera.MainCamera.Pitch -= yOffset;
                    
                //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
                Camera.MainCamera.Pitch = Math.Clamp(Camera.MainCamera.Pitch, -89.0f, 89.0f);
                
                Vector3 CameraDirection = Vector3.Zero;
                CameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                CameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                CameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                Camera.MainCamera.Front = Vector3.Normalize(CameraDirection);
            }
        }
    }
}