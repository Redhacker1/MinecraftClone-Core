#define Core
#if !Core
using Godot;
#endif

using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Input;
using Engine.MathLib;
using Engine.Objects;
using Engine.Physics;
using Engine.Rendering;
using MCClone_Core.World_CS.Blocks;
using MCClone_Core.World_CS.Generation;
using Silk.NET.Input;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

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


		void toggle_pause()
		{
			_paused =! _paused;
			#if !Core
			GetTree().Paused = _paused;
			Input.SetMouseMode(_paused ? Input.MouseMode.Visible : Input.MouseMode.Captured);
			#endif
		}

		public Player(Vector3 pos, Vector2 dir, Level level) : base(pos, dir, level)
		{
			
		}

		public override void _Ready()
		{
			FPCam = new Camera(new Vector3(Pos.X, Pos.Y + .8, Pos.Z), -System.Numerics.Vector3.UnitZ, System.Numerics.Vector3.UnitY,1600f/900f, true );
			//.FOV = 100;
			#if Core
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

		public override void _Process(double delta)
		{
			FPCam.Pos = new Vector3(Pos.X, Pos.Y + 0, Pos.Z);
			Freelook();
			
			Vector3 Location = Pos;
			HitResult result = Raycast.CastInDirection(Location,FPCam.Front, -1, 5);
			Vector3 pos = result.Location;
			
				
			if (InputHandler.KeyboardKeyDown(0, Key.E))
			{
				Console.WriteLine("Pressed");
				Vector3 norm = result.Normal;
				_on_Player_destroy_block(pos, norm);
			}

			if (InputHandler.KeyboardJustKeyPressed(0, Key.C))
			{
				Noclip = !Noclip;
			}


		}

		public override void _PhysicsProcess(double delta)
		{
			
			double cx = Math.Floor((Pos.X ) / ChunkCs.Dimension.X);
			double cz = Math.Floor((Pos.Z) / ChunkCs.Dimension.Z);
			double px = Pos.X - cx * ChunkCs.Dimension.X;
			double py = Pos.Y;
			double pz = Pos.Z - cz * ChunkCs.Dimension.Z;
			Vector3 forward = -Vector3.UnitZ;
			
			

			if (!_paused)
			{
				Vector3 Location = Pos;
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
								int by = (int) (MathHelper.Modulo(MathHelper.Round(pos.Y), ChunkCs.Dimension.Y) + .5);
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

			int cx = (int) Math.Floor(pos.X / ChunkCs.Dimension.X);
			int cz = (int) Math.Floor(pos.Z / ChunkCs.Dimension.Z);

			int bx = (int) (MathHelper.Modulo((float) Math.Floor(pos.X), ChunkCs.Dimension.X) + .5f);
			int by = (int) (MathHelper.Modulo((float) Math.Floor(pos.Y), ChunkCs.Dimension.Y) + .5f);
			int bz = (int) (MathHelper.Modulo((float) Math.Floor(pos.Z), ChunkCs.Dimension.Z) + .5f);
			
			Console.WriteLine(World == null);
			World?.change_block(cx, cz, bx, by, bz, 0);
		}
		
		
		void _on_Player_place_block(Vector3 pos, Vector3 norm, string type)
		{
			pos += norm * .5f;

			int cx = (int) Math.Floor(pos.X / ChunkCs.Dimension.X);
			int cz = (int) Math.Floor(pos.Z / ChunkCs.Dimension.Z);

			int bx = (int) (MathHelper.Modulo((float) Math.Floor(pos.X), ChunkCs.Dimension.X) + 0.5);
			int by = (int) (MathHelper.Modulo((float) Math.Floor(pos.Y), ChunkCs.Dimension.Y) + 0.5);
			int bz = (int) (MathHelper.Modulo((float) Math.Floor(pos.Z), ChunkCs.Dimension.Z) + 0.5);

			World?.change_block(cx, cz, bx, by, bz, _selectedBlockIndex);	
			
		}
		

		void _on_Player_highlight_block(Vector3 pos, Vector3 norm, float delta)
		{
			pos -= norm * .5f;
			
			int cx = (int) Math.Floor(pos.X / ChunkCs.Dimension.X);
			int cz = (int) Math.Floor(pos.Z / ChunkCs.Dimension.Z);

			int bx = (int) (MathHelper.Modulo((float) Math.Floor(pos.X), ChunkCs.Dimension.X) + 0.5);
			int by = (int) (MathHelper.Modulo((float) Math.Floor(pos.Y), ChunkCs.Dimension.Y) + 0.5);
			int bz = (int) (MathHelper.Modulo((float) Math.Floor(pos.Z), ChunkCs.Dimension.Z) + 0.5);


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
					Camera.MainCamera.Yaw -= xOffset;
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
