using System.Collections.Generic;
using Silk.NET.Input;

namespace Engine.Input.Default_Devices
{
    public class Keyboard : InputDevice
    {
        IKeyboard _keyboardDevice;
        
        Dictionary<Key, bool> _allKeys = new Dictionary<Key, bool>();
        Dictionary<Key, bool> _keysJustPressed = new Dictionary<Key, bool>();
        Dictionary<Key, bool> KeyRearmed = new Dictionary<Key, bool>();
        
        
        internal Keyboard(int id, IKeyboard keyboard, string name = "")
        {
            DeviceName = name;
            Id = id;
            _keyboardDevice = keyboard;
            
            foreach (Key key in _keyboardDevice.SupportedKeys)
            {
                _allKeys.Add(key, false);
                _keysJustPressed.Add(key, false);
                KeyRearmed.Add(key, false);
            }
        }
        
        
        public override void Poll()
        {
            foreach (Key currentKey in _allKeys.Keys)
            {
                bool keyPressed = _keyboardDevice.IsKeyPressed(currentKey);

                if (_keysJustPressed[currentKey] == false && keyPressed == true && KeyRearmed[currentKey])
                {
                    _keysJustPressed[currentKey] = true;
                    KeyRearmed[currentKey] = false;
                }
                else if (keyPressed == false && KeyRearmed[currentKey] == false)
                {
                    KeyRearmed[currentKey] = true;
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