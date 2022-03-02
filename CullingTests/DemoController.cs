using System.Numerics;
using Engine.Input;
using Engine.Rendering;
using Silk.NET.Input;

namespace CullingTests
{
    public class DemoController
    {
        readonly CharacterEntity pawn;
        Vector3 _velocity;
        float speed = 3;

        public DemoController(CharacterEntity character)
        {
            pawn = character;
        }
        public void Move(double deltatime)
        {
            Vector3 direction = new Vector3();
            Vector3 CameraForward = Camera.MainCamera.Front;
            Vector3 CameraLeft = -Camera.MainCamera.Right;

            if (InputHandler.KeyboardKeyDown(0,Key.W))
            {
                direction += CameraForward;
            }
            if (InputHandler.KeyboardKeyDown(0,Key.S))
            {
                direction -= CameraForward;
            }
                
            if (InputHandler.KeyboardKeyDown(0,Key.A))
            {
                direction -= CameraLeft;
            }
                
            if (InputHandler.KeyboardKeyDown(0,Key.D))
            {
                direction += CameraLeft;
            }
            
            
            _velocity.X = (direction.X * speed * (float)deltatime);
            _velocity.Z = (direction.Z * speed * (float)deltatime);
            if (pawn.Noclip)
            {
                _velocity.Y = (direction.Y * speed * (float)deltatime);
            }
            
            pawn.MoveRelative(_velocity.X, _velocity.Z, speed);
            pawn.Move(_velocity);
        }
    }
}