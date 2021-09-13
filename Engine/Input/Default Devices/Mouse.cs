using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.OpenGL;

namespace Engine.Input.Default_Devices
{
    public class Mouse : InputDevice
    {
        public Vector2 Position;
        public Vector2 Delta;
        Vector2 LastPos;
        
        Dictionary<MouseButton, bool> AllMouseKeys = new Dictionary<MouseButton, bool>();
        Dictionary<MouseButton, bool> MouseKeysJustPressed = new Dictionary<MouseButton, bool>();
        Dictionary<MouseButton, bool> MouseKeysJustReleased = new Dictionary<MouseButton, bool>();
        
        
        IMouse InputDevice;

        internal Mouse(int id, IMouse mouse, string name = "")
        {
            DeviceName = name;
            ID = id;
            InputDevice = mouse;

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
                if (MouseKeysJustReleased[mouseButtons] == false && KeyPressed == true)
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