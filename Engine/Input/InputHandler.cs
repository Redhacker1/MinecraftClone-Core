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
        static internal IInputContext Context;

        public static void InitInputHandler(IInputContext inputContext)
        {
            Context = inputContext;
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
            for (int i = 0; i < _keyboards.Count; i++)
            {
                _keyboards[i].Poll();
            }
            for (int i = 0; i < _mice.Count; i++)
            {
                _mice[i].Poll();
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

        public static void SetMouseMode(int id, CursorMode mode)
        {
            _mice[id].SetMouseMode(mode);
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