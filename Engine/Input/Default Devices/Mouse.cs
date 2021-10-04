using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Input;

namespace Engine.Input.Default_Devices
{
    public class Mouse : InputDevice
    {
        public Vector2 Position;
        public Vector2 Delta;
        Vector2 LastPos;
        

        readonly Dictionary<MouseButton, bool> AllMouseKeys = new();
        readonly Dictionary<MouseButton, bool> _mouseKeysJustPressed = new();
        readonly Dictionary<MouseButton, bool> MouseKeysJustReleased = new();
        
        
        IMouse InputDevice;

        internal Mouse(int id, IMouse mouse, string name = "")
        {
            DeviceName = name;
            ID = id;
            InputDevice = mouse;

            foreach (MouseButton buttons in mouse.SupportedButtons)
            {
                AllMouseKeys.Add(buttons, false);
                _mouseKeysJustPressed.Add(buttons, false);
                MouseKeysJustReleased.Add(buttons, false);
            }
            

        }

        internal void SetMouseMode(CursorMode mode)
        {
            InputDevice.Cursor.CursorMode = mode;
        }

        internal bool MouseButtonDown(MouseButton key)
        {
            return AllMouseKeys[key];
        }


        /// <summary>
        /// Polls the mouse to check the state of inputs
        /// </summary>
        public override void Poll()
        {
            // Mouse Input
            LastPos = Position;
            Position = InputDevice.Position;
            Delta = Position - LastPos;
            
            foreach (MouseButton mouseButtons in InputDevice.SupportedButtons)
            {
                bool KeyPressed = InputDevice.IsButtonPressed(mouseButtons);
                
                if (MouseKeysJustReleased[mouseButtons] == false && KeyPressed)
                {
                    MouseKeysJustReleased[mouseButtons] = true;   
                }
                else
                {
                    MouseKeysJustReleased[mouseButtons] = false;   
                }
                if (KeyPressed == false && AllMouseKeys[mouseButtons])
                {
                    MouseKeysJustReleased[mouseButtons] = false;
                }
                AllMouseKeys[mouseButtons] = KeyPressed;
            }
        }
        
    }
}