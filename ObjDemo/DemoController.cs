using Engine.Input;
using Engine.MathLib;
using Engine.MathLib.DoublePrecision_Numerics;
using Engine.Rendering;
using Silk.NET.Input;

namespace ObjDemo
{
    public class DemoController
    {
        readonly CharacterEntity _pawn;
        Vector3 _velocity;
        float _speed = 25.498592f;

        public DemoController(CharacterEntity character)
        {
            _pawn = character;
        }
        public void Move(double deltatime)
        {
            Vector3 direction = new Vector3();
            Vector3 cameraForward = Camera.MainCamera.Front.CastToDouble();
            Vector3 cameraLeft = -Camera.MainCamera.Right.CastToDouble();

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