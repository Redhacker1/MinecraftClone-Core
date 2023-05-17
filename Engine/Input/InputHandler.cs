using System;
using System.Collections.Generic;
using Engine.Utilities.MathLib;
using Engine.Windowing;
using SharpInterop.SDL2;

namespace Engine.Input
{
    public static class InputHandler
    {

        static readonly Dictionary<Keycode, KeyState> KeyStates = new Dictionary<Keycode, KeyState>();
        static Int2 _mousePos;
        static bool _trappedMouse;
        static Window _windowContext;
        static Int2 _lastmousePos;
        

        static Int2 _delta;

        public static void InitInputHandler(Window Window)
        {

            _windowContext = Window;

            _windowContext.KeyPressedEvent += KeyDown;
            _windowContext.KeyReleasedEvent += KeyUp;
            _windowContext.MouseMovedEvent += OnMouseMove;
            
            
            Keycode[] keys = Enum.GetValues<Keycode>();

            foreach (Keycode key in keys)
            {
                if (KeyStates.ContainsKey(key))
                    continue;
                KeyStates.Add(key, KeyState.Released);
            }


        }


        public static Dictionary<Keycode, KeyState> GetKeysPressedThisFrame()
        {
            return new Dictionary<Keycode, KeyState>(KeyStates);
        }

        static void OnMouseMove(Int2 absPos, Int2 relativePos)
        {
            _lastmousePos = _mousePos;
            _mousePos = absPos;
            _delta = relativePos;
        }

        static void KeyDown(Keycode keycode, KeyModifiers modifiers, char character, bool repeat)
        {
            if (!KeyStates.ContainsKey(keycode))
            {
                Console.WriteLine($"Error, unknown key {Enum.GetName(keycode)}");
                return;
            }
            
            KeyState state = KeyState.JustPressed;
            if (KeyStates[keycode] != KeyState.Released)
            {
                state = KeyState.Pressed;
            }
            KeyStates[keycode] = state;   

        }

        static void KeyUp(Keycode keycode)
        {
            
            if (!KeyStates.ContainsKey(keycode))
            {
                Console.WriteLine($"Error, unknown key {Enum.GetName((SDL.SDL_Keycode)keycode)}");
                return;
            }
            
            if (!KeyStates.ContainsKey(keycode))
            {
                return;
            }
            
            KeyStates[keycode] = KeyState.Released;    
            
        }
        
        public static void PollInputs()
        {
            foreach ((Keycode key, KeyState state) in KeyStates)
            {
                if (state == KeyState.JustPressed)
                {
                    KeyStates[key] = KeyState.Pressed;
                }
            }
        }
        


        public static Int2 MousePos()
        {
            return _mousePos;
        }
        
        public static Int2 MouseDelta()
        {
            return _delta;
        }

        public static void SetMouseMode(bool Shown, bool Trapped)
        {
            _windowContext.SetCursorMode(new MouseState()
            {
                MouseTrapped = Trapped,
                MouseVisible = Shown
            });
        }
        
        [Obsolete]
        public static (bool, bool) GetMouseMode(int id)
        {
            MouseState mouseMode = _windowContext.GetMouseMode();
            
            bool mouseVisibility = mouseMode.MouseVisible;

            return (mouseVisibility, mouseMode.MouseTrapped);
        }
        
        public static MouseState GetMouseMode()
        {
            return _windowContext.GetMouseMode();
        }
        
        

        /// <summary>
        /// Gets if the key is currently pressed or "down" on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key is currently pressed</returns>
        public static bool KeyboardKeyDown(int keyboard, Keycode desiredKey)
        {
            return KeyStates[desiredKey] == KeyState.JustPressed || KeyStates[desiredKey] == KeyState.Pressed;
        }
        
        /// <summary>
        /// Gets if the key was just pressed on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key was just pressed</returns>
        public static bool KeyboardJustKeyPressed(int keyboard, Keycode desiredKey)
        {
           return KeyStates[desiredKey] == KeyState.JustPressed;
        }
        
        /// <summary>
        /// Gets if the key is unpressed on the desired keyboard
        /// </summary>
        /// <param name="keyboard">which keyboard to use, if unsure leave at 0</param>
        /// <param name="desiredKey">Key you are testing for</param>
        /// <returns>Whether the key was just pressed</returns>
        public static bool KeyboardKeyUp(int keyboard, Keycode desiredKey)
        {
            return KeyStates[desiredKey] == KeyState.Released;
        }
    }
}