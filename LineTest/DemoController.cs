﻿using System.Numerics;
using Engine.Input;
using Engine.Rendering.Abstract;
using LineTest;
using Veldrid;

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
            Vector3 cameraForward = Camera.MainCamera.Front;
            Vector3 cameraLeft = -Camera.MainCamera.Right;

            if (InputHandler.KeyboardKeyDown(0, Keycode.W))
            {
                direction += cameraForward;
            }
            if (InputHandler.KeyboardKeyDown(0,Keycode.S))
            {
                direction -= cameraForward;
            }
                
            if (InputHandler.KeyboardKeyDown(0,Keycode.A))
            {
                direction -= cameraLeft;
            }
                
            if (InputHandler.KeyboardKeyDown(0,Keycode.D))
            {
                direction += cameraLeft;
            }


            _velocity.X = direction.X * _speed * (float)deltatime;
            _velocity.Z = direction.Z * _speed * (float)deltatime;
            if (_pawn.Noclip)
            {
                _velocity.Y = direction.Y * _speed * (float)deltatime;
            }

            _pawn.MoveRelative(_velocity.X, _velocity.Z, _speed);
            _pawn.Move(_velocity);
        }
    }
}