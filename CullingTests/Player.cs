using System;
using System.Numerics;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Silk.NET.Input;


namespace CullingTests
{
	//[Tool]
	public class Player: CharacterEntity
	{
		

		DemoController _controller;
		

		Camera _fpCam;


		public override void _Ready()
		{
			_fpCam = new Camera(Pos, -Vector3.UnitZ, Vector3.UnitY,1600f/900f, true );
			InputHandler.SetMouseMode(0, CursorMode.Raw);
			_controller = new DemoController(this);
		}

		public override void _Process(double delta)
		{
			_fpCam.Pos = Pos;
			Freelook();
		}

		public override void _PhysicsProcess(double delta)
		{
			_controller.Move(delta);
		}
		
		

		public void Freelook()
		{
			if (Camera.MainCamera != null)
			{
				float xOffset = InputHandler.MouseDelta(0).X * 0.1f;
				float yOffset = InputHandler.MouseDelta(0).Y * 0.1f;

				if (Camera.MainCamera != null)
				{
					Camera.MainCamera.Yaw += xOffset;
					Camera.MainCamera.Pitch -= yOffset;
                    
					//We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
					Camera.MainCamera.Pitch = Math.Clamp(Camera.MainCamera.Pitch, -89.0f, 89.0f);
                
					Engine.MathLib.DoublePrecision_Numerics.Vector3 cameraDirection = Engine.MathLib.DoublePrecision_Numerics.Vector3.Zero;
					cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
					cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
					cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
					Camera.MainCamera.Front = Engine.MathLib.DoublePrecision_Numerics.Vector3.Normalize(cameraDirection);
				}
			}
		}
	}
}
