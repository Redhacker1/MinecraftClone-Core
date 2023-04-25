using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace Engine.Input
{
    public class InputHandler
    {
        internal static Sdl2Window Context;

        static readonly Dictionary<Key, KeyState> KeyStates = new Dictionary<Key, KeyState>();
        static Vector2 _mousePos;
        static bool _trappedMouse;

        public static void InitInputHandler(Sdl2Window inputContext)
        {
            Context = inputContext;

            Context.KeyDown += KeyDown;
            Context.KeyUp += KeyUp;
            Context.MouseMove += OnMouseMove;

            Key[] keys = Enum.GetValues<Key>();

            foreach (Key key in keys)
            {
                if (KeyStates.ContainsKey(key))
                    continue;
                KeyStates.Add(key, KeyState.Released);
            }


        }

        static void OnMouseMove(MouseMoveEventArgs mouseData)
        {
            _mousePos = mouseData.MousePosition;
        }

        static void KeyDown(KeyEvent eventData)
        {
            if(eventData.Down)
            {
                KeyState state = KeyState.JustPressed;
                if (KeyStates[eventData.Key] != KeyState.Released && eventData.Down)
                {
                    state = KeyState.Pressed;
                }
                KeyStates[eventData.Key] = state;   
            }

        }

        static void KeyUp(KeyEvent eventData)
        {
            if (!eventData.Down)
            {
                KeyStates[eventData.Key] = KeyState.Released;    
            }
            
        }
        
        public static void PollInputs()
        {
            foreach ((Key key, KeyState state) in KeyStates)
            {
                if (state == KeyState.JustPressed)
                {
                    KeyStates[key] = KeyState.Pressed;
                }
            }
        }

        public static Action<char> KeyPressed = c => { };


        public static Vector2 MousePos()
        {
            return _mousePos;
        }
        
        public static Vector2 MouseDelta()
        {
            return Context.MouseDelta;
        }

        public static void SetMouseMode(bool Shown, bool Trapped)
        {
            Sdl2Native.SDL_ShowCursor(Shown ? 1 : 0);
            Sdl2Native.SDL_SetRelativeMouseMode(Trapped);
            _trappedMouse = Trapped;
        }
        
        public static (bool, bool) GetMouseMode(int id)
        {
            bool mouseVisibility = Sdl2Native.SDL_ShowCursor(Sdl2Native.SDL_QUERY) == Sdl2Native.SDL_ENABLE;

            return (mouseVisibility, _trappedMouse);
        }
        
        

        /// <summary>
        /// Gets if the key is currently pressed or "down" on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key is currently pressed</returns>
        public static bool KeyboardKeyDown(int keyboard, Key desiredKey)
        {
            return KeyStates[desiredKey] == KeyState.JustPressed || KeyStates[desiredKey] == KeyState.Pressed;
        }
        
        /// <summary>
        /// Gets if the key was just pressed on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key was just pressed</returns>
        public static bool KeyboardJustKeyPressed(int keyboard, Key desiredKey)
        {
           return KeyStates[desiredKey] == KeyState.JustPressed;
        }
        
        /// <summary>
        /// Gets if the key is unpressed on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key was just pressed</returns>
        public static bool KeyboardKeyUp(int keyboard, Key desiredKey)
        {
            return KeyStates[desiredKey] == KeyState.Released;
        }
    }
}