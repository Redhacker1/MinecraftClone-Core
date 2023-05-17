using System.Numerics;
using Engine.Input;
using Engine.Rendering.Abstract;

namespace VeldridCubeTest
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


            _velocity.X = (float)(direction.X * Player.Speed * deltatime);
            _velocity.Z = (float)(direction.Z * Player.Speed * deltatime);
            _velocity.Y = (float)(direction.Y * Player.Speed * deltatime);

            _pawn.MoveRelative(_velocity.X, _velocity.Z, Player.Speed);
            _pawn.Move(_velocity);
        }
    }
}