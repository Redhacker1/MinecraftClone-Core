

using System;
using System.Numerics;
using Engine.Debugging;
using Engine.Input;
using Engine.MathLib;
using Engine.Rendering.Abstract;
using MCClone_Core.Physics;
using MCClone_Core.World_CS.Blocks;
using MCClone_Core.World_CS.Generation;
using Silk.NET.Input;
using Raycast = MCClone_Core.Utility.Raycast;

namespace MCClone_Core.Player_CS
{
	//[Tool]
	public class Player: CharacterEntity
	{
		
		public ProcWorld World;

		const float MouseSensitivity = 0.3f;
		public const float Gravity = 9.8f;
		float _cameraXRotation;

		string _selectedBlock = string.Empty;
		
		public const double Speed = 5.498592;
		public const int JumpVel = 5;
		byte _selectedBlockIndex;
		

		PlayerController _controller;

		bool _paused;

		Camera FPCam;
		bool MoveMouse;


		void toggle_pause()
		{
			_paused =! _paused;
			#if !Core
			GetTree().Paused = _paused;
			Input.SetMouseMode(_paused ? Input.MouseMode.Visible : Input.MouseMode.Captured);
			#endif
		}

		public Player(Vector3 pos, Vector2 dir) : base(pos, dir)
		{
			ConsoleLibrary.BindCommand("Noclip", "Enables or Disables noclip", "Noclip", _ =>
			{
				ToggleNoclip();
				return "";
			});
		}

		public override void _Ready()
		{
			PhysicsTick = true;

			FPCam = new Camera(new Transform(), -Vector3.UnitZ, Vector3.UnitY,1600f/900f, true )
			{
				Position = new Vector3(Position.X, Position.Y + .8f, Position.Z)
			};
			//.FOV = 100;
			#if Core
			MoveMouse = true;
			InputHandler.SetMouseMode(0, CursorMode.Raw);
			#else
				SetPos(new Vector3(Translation.x, Translation.y, Translation.z));
			#endif
			_controller = new PlayerController(this);

			_selectedBlock = BlockHelper.IdToString[_selectedBlockIndex];

			#if Core
			#else
			_console = GetNode("CameraBase/Camera/Control") as Control;

			_fpCam = GetNode<Camera>("CameraBase/Camera");
			_raycast = GetNode<RayCast>("CameraBase/Camera/RayCast");
			_infoLabel = GetNode<Label>("CameraBase/Camera/Debug_line_01");

			if (!Engine.EditorHint)
			{
				Input.SetMouseMode(Input.MouseMode.Captured);	
			}'
			#endif
		}

		protected override void _Process(double delta)
		{

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
			
			
			FPCam.Position = new Vector3(Position.X, Position.Y + 0, Position.Z);
			if (MoveMouse)
			{
				Freelook();
			}

			Vector3 Location = Position;
			HitResult result = Raycast.CastInDirection(Location,FPCam.Front, -1, 5);
			Vector3 pos = result.Location;


			if (MoveMouse)
			{
				if (InputHandler.KeyboardKeyDown(0, Key.E))
				{
					Vector3 norm = result.Normal;
					_on_Player_destroy_block(pos, norm);
				}

				if (InputHandler.KeyboardJustKeyPressed(0, Key.C))
				{
					var test = Noclip ? "Enabled" : "Disabled";
					Console.WriteLine($"Noclip {test}");
					ConsoleLibrary.SendCommand($"Noclip");
				}
			}


		}


		void ToggleNoclip()
		{
			Noclip = !Noclip;
		}

		protected override void _PhysicsProcess(double delta)
		{
			
			double cx = Math.Floor(Position.X / ChunkCs.MaxX);
			double cz = Math.Floor(Position.Z / ChunkCs.MaxZ);
			double px = Position.X - cx * ChunkCs.MaxX;
			double py = Position.Y;
			double pz = Position.Z - cz * ChunkCs.MaxZ;
			Vector3 forward = Vector3.UnitZ;
			
			
			if (!_paused && MoveMouse)
			{
				Vector3 Location = Position;
				HitResult result = Raycast.CastInDirection(Location,forward, -1, 5);
				Vector3 pos = result.Location;

				#if Core
				_controller.Player_move(delta);
				
				if (InputHandler.KeyboardJustKeyPressed(0, Key.E))
				{
					Console.WriteLine("Pressed");
					Vector3 norm = result.Normal;
					_on_Player_destroy_block(pos, norm);
				}
				#else
				if (!Engine.EditorHint)
				{
					_controller.Player_move(delta);
					WorldScript.lines.DrawBlock((int) pos.X, (int) pos.Y, (int) pos.Z, delta);
				}
				#endif

				if (result.Hit)
				{

					
					#if Core
					// TODO: Implement Line tracing and debug lines before re-enabling this code!
					#else
					_on_Player_highlight_block(pos, norm, delta);

					if (!Engine.EditorHint)
					{
						if (Input.IsActionJustPressed("click"))
						{
							_on_Player_destroy_block(pos, norm);
						}
						else if(Input.IsActionJustPressed("right_click"))
						{
							WorldScript.lines.DrawRay(_fpCam.Transform.origin, (forward * 5).CastToGodot(), Colors.Red, delta);

							if (Vector3.Distance(pos, Translation.CastToCore()) > 1.2)
							{
								int by = (int) (MathHelper.Modulo(MathHelper.Round(pos.Y), ChunkCs.MaxY) + .5);
								_on_Player_place_block(pos,norm, _selectedBlock);
								if (!OnGround )
								{
									Translation = new Vector3(Translation.x, by + .5f, Translation.z).CastToGodot();   
								}
							}
						
						}	
					}
					#endif
				}
			}
		}
		
		void _on_Player_destroy_block(Vector3 pos, Vector3 norm)
		{
			pos += norm * .5f;

			int cx = (int) Math.Floor(pos.X / ChunkCs.MaxX);
			int cz = (int) Math.Floor(pos.Z / ChunkCs.MaxZ);

			int bx = (int) (MathHelper.Modulo((float) Math.Floor(pos.X), ChunkCs.MaxX) + .5f);
			int by = (int) (MathHelper.Modulo((float) Math.Floor(pos.Y), ChunkCs.MaxY) + .5f);
			int bz = (int) (MathHelper.Modulo((float) Math.Floor(pos.Z), ChunkCs.MaxZ) + .5f);
			
			Console.WriteLine(World == null);
			World?.change_block(cx, cz, bx, by, bz, 0);
		}
		
		
		void _on_Player_place_block(Vector3 pos, Vector3 norm, string type)
		{
			pos += norm * .5f;

			int cx = (int) Math.Floor(pos.X / ChunkCs.MaxX);
			int cz = (int) Math.Floor(pos.Z / ChunkCs.MaxZ);

			int bx = (int) (MathHelper.Modulo((float) Math.Floor(pos.X), ChunkCs.MaxX) + 0.5);
			int by = (int) (MathHelper.Modulo((float) Math.Floor(pos.Y), ChunkCs.MaxY) + 0.5);
			int bz = (int) (MathHelper.Modulo((float) Math.Floor(pos.Z), ChunkCs.MaxZ) + 0.5);

			World?.change_block(cx, cz, bx, by, bz, _selectedBlockIndex);	
			
		}
		

		void _on_Player_highlight_block(Vector3 pos, Vector3 norm, float delta)
		{
			pos -= norm * .5f;
			
			int cx = (int) Math.Floor(pos.X / ChunkCs.MaxX);
			int cz = (int) Math.Floor(pos.Z / ChunkCs.MaxZ);

			int bx = (int) (MathHelper.Modulo((float) Math.Floor(pos.X), ChunkCs.MaxX) + 0.5);
			int by = (int) (MathHelper.Modulo((float) Math.Floor(pos.Y), ChunkCs.MaxY) + 0.5);
			int bz = (int) (MathHelper.Modulo((float) Math.Floor(pos.Z), ChunkCs.MaxZ) + 0.5);


		}
		
		public void _on_Player_unhighlight_block()
		{
			
		}
		

		public void Freelook()
		{
			if (!_paused && Camera.MainCamera != null)
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
