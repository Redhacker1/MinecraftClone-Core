using System;
using Engine.Input;
using Engine.MathLib.DoublePrecision_Numerics;
using Engine.Objects;
using Engine.Rendering;
using Silk.NET.Input;

namespace CullingTests
{
    public class DemoController
    {
        readonly CharacterEntity pawn;
        Vector3 _velocity = new Vector3();
        float speed = 25.498592f;

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


            _velocity.X = (direction.X * speed * deltatime);
            _velocity.Z = (direction.Z * speed * deltatime);
            if (pawn.Noclip)
            {
                _velocity.Y = (direction.Y * speed * deltatime);
            }

            pawn.MoveRelative(_velocity.X, _velocity.Z, speed);
            pawn.Move(_velocity);
        }
    }
}