using System.Collections.Generic;
using Silk.NET.Input;

namespace Engine.Input.Default_Devices
{
    public sealed class Keyboard : InputDevice
    {
        readonly IKeyboard _keyboardDevice;

        readonly Dictionary<Key, bool> _allKeys = new Dictionary<Key, bool>();
        readonly Dictionary<Key, bool> _keysJustPressed = new Dictionary<Key, bool>();
        readonly Dictionary<Key, bool> _keyRearmed = new Dictionary<Key, bool>();
        
        
        internal Keyboard(int id, IKeyboard keyboard, string name = "")
        {
            DeviceName = name;
            Id = id;
            _keyboardDevice = keyboard;
            
            foreach (Key key in _keyboardDevice.SupportedKeys)
            {
                _allKeys.Add(key, false);
                _keysJustPressed.Add(key, false);
                _keyRearmed.Add(key, false);
            }
        }
        
        
        public override void Poll()
        {
            foreach (Key currentKey in _allKeys.Keys)
            {
                bool keyPressed = _keyboardDevice.IsKeyPressed(currentKey);

                if (_keysJustPressed[currentKey] == false && keyPressed && _keyRearmed[currentKey])
                {
                    _keysJustPressed[currentKey] = true;
                    _keyRearmed[currentKey] = false;
                }
                else if (keyPressed == false && _keyRearmed[currentKey] == false)
                {
                    _keyRearmed[currentKey] = true;
                }
                else
                {
                    _keysJustPressed[currentKey] = false;   
                }
                _allKeys[currentKey] = keyPressed;
            }
        }
        
        
        public bool KeyPressed(Key desiredKey)
        {
            return _allKeys[desiredKey];
        }
        
        public bool KeyJustPressed(Key desiredKey)
        {
            return _keysJustPressed[desiredKey];
        }
        
        public bool KeyJustReleased(Key desiredKey)
        {
            return _keysJustPressed[desiredKey] == false && _allKeys[desiredKey] == false;
        }
    }
}