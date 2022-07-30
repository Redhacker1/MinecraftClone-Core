#define Core
#if !Core
using Godot;
#endif

using System;
using System.Numerics;
using Engine.Input;
using Engine.MathLib;
using Engine.Rendering.Abstract;
using Silk.NET.Input;

namespace ObjDemo
{
	public class Player: CharacterEntity
	{
		public const double Speed = 5.498592;
		DemoController _controller;
		

		Camera _fpCam;


		public override void _Ready()
		{
			Rotation = Quaternion.Identity;
			_fpCam = new Camera(Position, -Vector3.UnitZ, Vector3.UnitY,1600f/900f, true )
			{
				Rotation = Rotation
			};
			InputHandler.SetMouseMode(0, CursorMode.Raw);
			MoveMouse = true;
			_controller = new DemoController(this);
		}

		public override void _Process(double delta)
		{
			_fpCam.Pos = Position;
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
                
					Vector3 CameraDirection = Vector3.Zero;
					CameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
					CameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
					CameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
					Camera.MainCamera.Front = Vector3.Normalize(CameraDirection);
				}
			}
		}
	}
}
