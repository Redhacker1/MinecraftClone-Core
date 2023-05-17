#define Core

#if !Core
using Godot;
#endif
using System;
using System.Diagnostics;
using System.Numerics;
using Engine.Input;
using Engine.Rendering.Abstract;
using Veldrid;

namespace MCClone_Core.Player_CS
{
    public class PlayerController
    {
        readonly Player pawn;

        public PlayerController(Player PawnReference)
        {
            pawn = PawnReference;
        }

        Stopwatch _stopwatch;
        int counter = 0;

        public void Player_move(double delta)
        {
            Vector3 velocity = Vector3.Zero;   

            Vector3 direction = new Vector3();

            Vector3 CameraForward = Camera.MainCamera.Front;
            Vector3 CameraLeft = -Camera.MainCamera.Right;

            if (InputHandler.KeyboardKeyDown(0, Keycode.W))
            {
                direction += CameraForward;
            }

            if (InputHandler.KeyboardKeyDown(0, Keycode.S))
            {
                direction -= CameraForward;
            }

            if (InputHandler.KeyboardKeyDown(0, Keycode.A))
            {
                direction -= CameraLeft;
            }

            if (InputHandler.KeyboardKeyDown(0, Keycode.D))
            {
                direction += CameraLeft;
            }


            if (InputHandler.KeyboardKeyDown(0, Keycode.Space) && pawn.OnGround)
            {
                velocity.Y = 6f;
            }
            else if (!pawn.OnGround && !pawn.Noclip)
            {
                velocity.Y -= .2f * (float)delta;
            }
            else
            {
                velocity.Y = 0;
            }

            direction = Vector3.Normalize(direction);

            if (float.IsNaN(direction.LengthSquared()))
            {
                direction = Vector3.Zero;
            }

            velocity.X = (float)(direction.X * Player.Speed * delta);
            velocity.Z = (float)(direction.Z * Player.Speed * delta);
            velocity.Y = (float)(direction.Y * Player.Speed * delta);
            
            
            pawn.MoveRelative(velocity.X, velocity.Z, Player.Speed);
            pawn.Move(velocity, pawn.Position);

        }
    }
}