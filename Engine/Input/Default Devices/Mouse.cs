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
        Vector2 _lastPos;
        

        readonly Dictionary<MouseButton, bool> _allMouseKeys = new Dictionary<MouseButton, bool>();
        readonly Dictionary<MouseButton, bool> _mouseKeysJustPressed = new Dictionary<MouseButton, bool>();
        readonly Dictionary<MouseButton, bool> _mouseKeysJustReleased = new Dictionary<MouseButton, bool>();
        
        
        IMouse _inputDevice;

        internal Mouse(int id, IMouse mouse, string name = "")
        {
            DeviceName = name;
            Id = id;
            _inputDevice = mouse;

            foreach (MouseButton buttons in mouse.SupportedButtons)
            {
                _allMouseKeys.Add(buttons, false);
                _mouseKeysJustPressed.Add(buttons, false);
                _mouseKeysJustReleased.Add(buttons, false);
            }
            

        }

        internal void SetMouseMode(CursorMode mode)
        {
            _inputDevice.Cursor.CursorMode = mode;
        }

        internal bool MouseButtonDown(MouseButton key)
        {
            return _allMouseKeys[key];
        }


        /// <summary>
        /// Polls the mouse to check the state of inputs
        /// </summary>
        public override void Poll()
        {
            // Mouse Input
            _lastPos = Position;
            Position = _inputDevice.Position;
            Delta = Position - _lastPos;
            
            foreach (MouseButton mouseButtons in _inputDevice.SupportedButtons)
            {
                bool keyPressed = _inputDevice.IsButtonPressed(mouseButtons);
                
                if (_mouseKeysJustReleased[mouseButtons] == false && keyPressed == true)
                {
                    _mouseKeysJustReleased[mouseButtons] = true;   
                }
                else
                {
                    _mouseKeysJustReleased[mouseButtons] = false;   
                }
                if (keyPressed == false && _allMouseKeys[mouseButtons])
                {
                    _mouseKeysJustReleased[mouseButtons] = false;
                }
                _allMouseKeys[mouseButtons] = keyPressed;
            }
        }
        
    }
}