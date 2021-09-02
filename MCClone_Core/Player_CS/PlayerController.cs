#define Core

#if !Core
using Godot;
#endif
using Engine.Input;
using Engine.Rendering;
using Silk.NET.Input;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace MinecraftClone.Player_CS
{
    public class PlayerController
    {
        readonly Player pawn;
        Vector3 _velocity;
        public PlayerController(Player PawnReference)
        {
            pawn = PawnReference;
        }

        public void Player_move(float delta)
        {
            Vector3 direction = new Vector3();
        #if Core
            //direction.X = 1;
            //direction.Z = 1;
            
            Vector3 CameraForward = Camera.MainCamera.Front;
            Vector3 CameraLeft = -Camera.MainCamera.Right;

            if (InputHandler.KeyPressed(Key.W))
            {
                direction += CameraForward;
            }
            if (InputHandler.KeyPressed(Key.S))
            {
                direction -= CameraForward;
            }
                
            if (InputHandler.KeyPressed(Key.A))
            {
                direction -= CameraLeft;
            }
                
            if (InputHandler.KeyPressed(Key.D))
            {
                direction += CameraLeft;
            }


            if (InputHandler.KeyPressed(Key.Space)&& pawn.OnGround)
            {
                _velocity.Y = 6f * delta;
            }
            
        #else
            Basis cameraBaseBasis = pawn.Transform.basis;


            if (Input.IsActionPressed("forward"))
            {
                direction -= cameraBaseBasis.z.CastToCore();
            }
            if (Input.IsActionPressed("backward"))
            {
                direction += cameraBaseBasis.z.CastToCore();
            }
                
            if (Input.IsActionPressed("left"))
            {
                direction -= cameraBaseBasis.x.CastToCore();
            }
                
            if (Input.IsActionPressed("right"))
            {
                direction += cameraBaseBasis.x.CastToCore();
            }


            if (Input.IsActionPressed("jump") && pawn.OnGround)
            {
                _velocity.Y = 6f * delta;
            }
        #endif
            //float xa = 0.0f, ya = 0.0f;
            
            if (!pawn.OnGround && !pawn.Noclip)
            {
              _velocity.Y -= .2f * delta;   
            }
            else
            {
                if (InputHandler.KeyPressed(Key.Space))
                {
                    _velocity.Y = 6f * delta;
                }
                else
                {
                    _velocity.Y = 0;                    
                }

            }
            
            _velocity.X = (float) (direction.X * Player.Speed * delta);
            _velocity.Z = (float) (direction.Z * Player.Speed * delta);
            if (pawn.Noclip)
            {
                _velocity.Y = (float) (direction.Y * Player.Speed * delta);
            }
            
            pawn.MoveRelative(_velocity.X, _velocity.Z, Player.Speed);
            pawn.Move(_velocity);
            //pawn.Pos = _velocity;
        }
    }
}