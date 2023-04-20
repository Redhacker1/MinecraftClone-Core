using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace Engine.Input
{
    public class InputHandler
    {
        internal static Sdl2Window Context;

        static Dictionary<Key, KeyState> _keyStates = new Dictionary<Key, KeyState>();
        static Vector2 mousePos;
        static bool TrappedMouse = false;

        public static void InitInputHandler(Sdl2Window inputContext)
        {
            Context = inputContext;

            Context.KeyDown += KeyDown;
            Context.KeyUp += KeyUp;
            Context.MouseMove += OnMouseMove;

            var keys = Enum.GetValues<Key>();

            foreach (Key key in keys)
            {
                if (_keyStates.ContainsKey(key))
                    continue;
                _keyStates.Add(key, KeyState.Released);
            }


        }

        static void OnMouseMove(MouseMoveEventArgs mouseData)
        {
            mousePos = mouseData.MousePosition;
        }

        static void KeyDown(KeyEvent eventData)
        {
            _keyStates[eventData.Key] = KeyState.Released;
            
        }

        static void KeyUp(KeyEvent eventData)
        {
            _keyStates[eventData.Key] = eventData.Repeat ? KeyState.Pressed : KeyState.JustPressed;
        }
        
        [Obsolete]
        public static void PollInputs()
        {
        }
        

        public static Vector2 MousePos()
        {
            return mousePos;
        }
        
        public static Vector2 MouseDelta()
        {
            return Context.MouseDelta;
        }

        public static void SetMouseMode(bool Shown, bool Trapped)
        {
            Sdl2Native.SDL_ShowCursor(Shown ? 1 : 0);
            Sdl2Native.SDL_SetRelativeMouseMode(Trapped);
            TrappedMouse = Trapped;
        }
        
        public static (bool, bool) GetMouseMode(int id)
        {
            bool mouseVisibility = Sdl2Native.SDL_ShowCursor(Sdl2Native.SDL_QUERY) == Sdl2Native.SDL_ENABLE;

            return (mouseVisibility, TrappedMouse);
        }
        
        

        /// <summary>
        /// Gets if the key is currently pressed or "down" on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key is currently pressed</returns>
        public static bool KeyboardKeyDown(int keyboard, Key desiredKey)
        {
            return _keyStates[desiredKey] == KeyState.Released;
        }
        
        /// <summary>
        /// Gets if the key was just pressed on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key was just pressed</returns>
        public static bool KeyboardJustKeyPressed(int keyboard, Key desiredKey)
        {
           return _keyStates[desiredKey] == KeyState.JustPressed;
        }
        
        /// <summary>
        /// Gets if the key was just pressed on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key was just pressed</returns>
        public static bool KeyboardKeyPressed(int keyboard, Key desiredKey)
        {
            return _keyStates[desiredKey] == KeyState.Pressed;
        }
    }
}