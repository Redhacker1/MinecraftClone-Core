using Engine.Input;
using Engine.MathLib.DoublePrecision_Numerics;
using Engine.Objects;
using Engine.Rendering;
using Engine.Rendering.Shared.Culling;
using Silk.NET.Input;

namespace CullingTests
{
    public class DemoController
    {
        readonly CharacterEntity _pawn;
        Vector3 _velocity;
        float _speed = 3;

        public DemoController(CharacterEntity character)
        {
            _pawn = character;
        }
        public void Move(double deltatime)
        {
            Vector3 direction = new Vector3();
            Vector3 cameraForward = Camera.MainCamera.Front;
            Vector3 cameraLeft = -Camera.MainCamera.Right;

            if (InputHandler.KeyboardKeyDown(0,Key.W))
            {
                direction += cameraForward;
            }
            if (InputHandler.KeyboardKeyDown(0,Key.S))
            {
                direction -= cameraForward;
            }
                
            if (InputHandler.KeyboardKeyDown(0,Key.A))
            {
                direction -= cameraLeft;
            }
                
            if (InputHandler.KeyboardKeyDown(0,Key.D))
            {
                direction += cameraLeft;
            }
            
            
            _velocity.X = (direction.X * _speed * deltatime);
            _velocity.Z = (direction.Z * _speed * deltatime);
            if (_pawn.Noclip)
            {
                _velocity.Y = (direction.Y * _speed * deltatime);
            }
            
            _pawn.MoveRelative(_velocity.X, _velocity.Z, _speed);
            _pawn.Move(_velocity);
        }
    }
}