using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Input.Default_Devices;
using Engine.MathLib;
using Engine.Rendering;
using Silk.NET.Input;

namespace Engine.Input
{
    public class InputHandler
    {
        static List<Keyboard> _keyboards = new List<Keyboard>();
        static List<Mouse> _mice = new List<Mouse>();
        static IInputContext _context;

        public static void InitInputHandler(IInputContext inputContext)
        {
            _context = inputContext;
            for (int i = 0; i < inputContext.Keyboards.Count; i++)
            {
                _keyboards.Add(new Keyboard(i, inputContext.Keyboards[i]));
            }
            
            for (int i = 0; i < inputContext.Mice.Count; i++)
            {
                _mice.Add(new Mouse(i, inputContext.Mice[i]));
            }


        }

        public static void PollInputs()
        {

        }


        // TODO: Port over to player class, moving away from hardcoded input handling
        private static void OnMouseMove(IMouse mouse, Vector2 position)
        {
            float lookSensitivity = 0.1f;
            if (LastMousePosition == default) { LastMousePosition = position; }
            else
            {
                float xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
                float yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
                LastMousePosition = position;

                if (Camera.MainCamera != null)
                {
                    Camera.MainCamera.Yaw += xOffset;
                    Camera.MainCamera.Pitch -= yOffset;
                    
                    //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
                    Camera.MainCamera.Pitch = Math.Clamp(Camera.MainCamera.Pitch, -89.0f, 89.0f);
                
                    Vector3 CameraDirection = Vector3.Zero;
                    CameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                    CameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                    CameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                    Camera.MainCamera.Front = Vector3.Normalize(CameraDirection);
                }
                
            }
        }

        public static Vector2 MousePos(int id)
        {
            return _mice[id].Position;
        }
        
        public static Vector2 MouseDelta(int id)
        {
            return _mice[id].Delta;
        }
        
        

        /// <summary>
        /// Gets if the key is currently pressed or "down" on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key is currently pressed</returns>
        public static bool KeyboardKeyDown(int keyboard, Key desiredKey)
        {
            return _keyboards[keyboard].KeyPressed(desiredKey);
        }
        
        /// <summary>
        /// Gets if the key was just pressed on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key was just pressed</returns>
        public static bool KeyboardJustKeyPressed(int keyboard, Key desiredKey)
        {
           return _keyboards[keyboard].KeyJustPressed(desiredKey);
        }
        
        /// <summary>
        /// Gets if the key was just released on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key was just released</returns>
        public static bool KeyboardJustKeyReleased(int keyboard, Key desiredKey)
        {
            return _keyboards[keyboard].KeyJustReleased(desiredKey);
        }
    }
}