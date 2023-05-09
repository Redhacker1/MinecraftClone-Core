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
        Vector3 _velocity;

        public PlayerController(Player PawnReference)
        {
            pawn = PawnReference;
        }

        Stopwatch _stopwatch;
        int counter = 0;

        public void Player_move(double delta)
        {


            Vector3 direction = new Vector3();
            //direction.X = 1;
            //direction.Z = 1;

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
                _velocity.Y = 6f * (float)delta;
            }
            else if (!pawn.OnGround && !pawn.Noclip)
            {
                _velocity.Y -= .2f * (float)delta;
            }
            else
            {
                if (InputHandler.KeyboardKeyDown(0, Keycode.Space))
                {
                    _velocity.Y = 6f * (float)delta;
                }
                else
                {
                    _velocity.Y = 0;
                }

            }

            _velocity.X = (float)(direction.X * Player.Speed * delta);
            _velocity.Z = (float)(direction.Z * Player.Speed * delta);
            _velocity.Y = (float)(direction.Y * Player.Speed * delta);
            
            
            
            pawn.MoveRelative(_velocity.X, _velocity.Z, Player.Speed);
            pawn.Move(_velocity);
            
        }
    }
}