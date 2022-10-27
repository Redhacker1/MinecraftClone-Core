#define Core

#if !Core
using Godot;
#endif
using System.Numerics;
using Engine.Input;
using Engine.Rendering.Abstract;
using Silk.NET.Input;

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

        public void Player_move(double delta)
        {
            Vector3 direction = new Vector3();
            //direction.X = 1;
            //direction.Z = 1;

            Vector3 CameraForward = Camera.MainCamera.Front;
            Vector3 CameraLeft = -Camera.MainCamera.Right;

            if (InputHandler.KeyboardKeyDown(0, Key.W))
            {
                direction += CameraForward;
            }

            if (InputHandler.KeyboardKeyDown(0, Key.S))
            {
                direction -= CameraForward;
            }

            if (InputHandler.KeyboardKeyDown(0, Key.A))
            {
                direction -= CameraLeft;
            }

            if (InputHandler.KeyboardKeyDown(0, Key.D))
            {
                direction += CameraLeft;
            }


            if (InputHandler.KeyboardKeyDown(0, Key.Space) && pawn.OnGround)
            {
                _velocity.Y = 6f * (float)delta;
            }
            else if (!pawn.OnGround && !pawn.Noclip)
            {
                _velocity.Y -= .2f * (float)delta;
            }
            else
            {
                if (InputHandler.KeyboardKeyDown(0, Key.Space))
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