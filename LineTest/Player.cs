#define Core
#if !Core
using Godot;
#endif

using System.Numerics;
using Engine.Input;
using Engine.MathLib;
using Engine.Rendering.Abstract;
using LineTest;
using Silk.NET.Input;

namespace ObjDemo
{
	//[Tool]
	public class Player: CharacterEntity
	{
		
		DemoController _controller;
		

		Camera _fpCam;


		protected override void _Ready()
		{
			Console.WriteLine("Ready");
			Rotation = Quaternion.Identity;
			_fpCam = new Camera(this.WorldTransform, -Vector3.UnitZ, Vector3.UnitY,1600f/900f, true );
			_fpCam.Rotation = Quaternion.Identity;
			InputHandler.SetMouseMode(0, CursorMode.Raw);
			_controller = new DemoController(this);
			Camera.MainCamera = _fpCam;
		}

		protected override void _Process(double delta)
		{
			_fpCam.Position = Position;
			if (InputHandler.KeyboardJustKeyPressed(0,Key.Escape))
			{
				if (MoveMouse)
				{
					InputHandler.SetMouseMode(0, CursorMode.Normal);
					MoveMouse = false;
				}
				else
				{
					InputHandler.SetMouseMode(0, CursorMode.Raw);
					MoveMouse = true;
				}
				
			}

			if (MoveMouse)
			{
				Freelook();				
			}

		}

		public bool MoveMouse { get; set; }

		protected override void _PhysicsProcess(double delta)
		{
			_controller.Move(delta);
		}
		
		

		public void Freelook()
		{
			if (Camera.MainCamera != null)
			{
				float xOffset = InputHandler.MouseDelta(0).X * 0.1f;
				float yOffset = InputHandler.MouseDelta(0).Y * 0.1f;
				
				Camera.MainCamera.Yaw += xOffset;
				Camera.MainCamera.Pitch -= yOffset;
                    
				//We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
				Camera.MainCamera.Pitch = Math.Clamp(Camera.MainCamera.Pitch, -89.0f, 89.0f);
                
				Vector3 cameraDirection = Vector3.Zero;
				cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
				cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
				cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
				Camera.MainCamera.Front = Vector3.Normalize(cameraDirection);
			}
		}
	}
}
