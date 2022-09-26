#define Core
#if !Core
using Godot;
#endif

using System.Numerics;
using Engine.Input;
using Engine.MathLib;
using Engine.Rendering.Abstract;
using Silk.NET.Input;

namespace VeldridCubeTest
{
	//[Tool]
	public class Player: CharacterEntity
	{
		
		public const double Speed = 5.498592;
		DemoController _controller;
		

		Camera _fpCam;


		public override void _Ready()
		{
			_fpCam = new Camera(new Transform(), -Vector3.UnitZ, Vector3.UnitY,1600f/900f, true );
			AddChild(_fpCam);
			
			InputHandler.SetMouseMode(0, CursorMode.Raw);
			_controller = new DemoController(this);

			Ticks = true;
			PhysicsTick = true;
		}

		bool _isPaused;

		protected override void _Process(double delta)
		{
			if (InputHandler.KeyboardJustKeyPressed(0, Key.Escape))
			{
				_isPaused = !_isPaused;
				InputHandler.SetMouseMode(0, _isPaused ? CursorMode.Normal : CursorMode.Raw);
			}

			if (!_isPaused)
			{
				Freelook();	
			}
		}

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
