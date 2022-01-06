#define Core

#if !Core
using Godot;
#endif
using Engine.Input;
using Engine.Rendering;
using Engine.Rendering.Shared.Culling;
using Silk.NET.Input;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace MCClone_Core.Player_CS
{
    public class PlayerController
    {
        readonly Player _pawn;
        Vector3 _velocity;

        public PlayerController(Player pawnReference)
        {
            _pawn = pawnReference;
        }

        public void Player_move(double delta)
        {
            Vector3 direction = new Vector3();
            //direction.X = 1;
            //direction.Z = 1;

            Vector3 cameraForward = Camera.MainCamera.Front;
            Vector3 cameraLeft = -Camera.MainCamera.Right;

            if (InputHandler.KeyboardKeyDown(0, Key.W))
            {
                direction += cameraForward;
            }

            if (InputHandler.KeyboardKeyDown(0, Key.S))
            {
                direction -= cameraForward;
            }

            if (InputHandler.KeyboardKeyDown(0, Key.A))
            {
                direction -= cameraLeft;
            }

            if (InputHandler.KeyboardKeyDown(0, Key.D))
            {
                direction += cameraLeft;
            }


            if (InputHandler.KeyboardKeyDown(0, Key.Space) && _pawn.OnGround)
            {
                _velocity.Y = 6f * delta;
            }

            if (!_pawn.OnGround && !_pawn.Noclip)
            {
                _velocity.Y -= .2f * delta;
            }
            else
            {
                if (InputHandler.KeyboardKeyDown(0, Key.Space))
                {
                    _velocity.Y = 6f * delta;
                }
                else
                {
                    _velocity.Y = 0;
                }

            }

            _velocity.X = (float)(direction.X * Player.Speed * delta);
            _velocity.Z = (float)(direction.Z * Player.Speed * delta);
            if (_pawn.Noclip)
            {
                _velocity.Y = (float)(direction.Y * Player.Speed * delta);
            }

            _pawn.MoveRelative(_velocity.X, _velocity.Z, Player.Speed);
            _pawn.Move(_velocity);
            //pawn.Pos = _velocity;
        }
    }
}