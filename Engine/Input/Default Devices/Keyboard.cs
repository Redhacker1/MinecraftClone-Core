using System.Collections.Generic;
using Silk.NET.Input;

namespace Engine.Input.Default_Devices
{
    public class Keyboard : InputDevice
    {
        IKeyboard keyboardDevice;
        
        Dictionary<Key, bool> AllKeys = new Dictionary<Key, bool>();
        Dictionary<Key, bool> KeysJustPressed = new Dictionary<Key, bool>();
        Dictionary<Key, bool> KeysJustReleased = new Dictionary<Key, bool>();
        Dictionary<Key, bool> KeysReleased = new Dictionary<Key, bool>();
        
        
        internal Keyboard(int id, IKeyboard keyboard, string name = "")
        {
            DeviceName = name;
            ID = id;
            keyboardDevice = keyboard;
            
            foreach (Key key in keyboardDevice.SupportedKeys)
            {
                AllKeys.Add(key, false);
                KeysJustPressed.Add(key, false);
                KeysReleased.Add(key, false);
                KeysJustReleased.Add(key, false);
            }
        }
        
        
        public override void Poll()
        {
            foreach (Key CurrentKey in AllKeys.Keys)
            {
                bool KeyPressed = keyboardDevice.IsKeyPressed(CurrentKey);

                if (KeysJustPressed[CurrentKey] == false && KeyPressed && KeysReleased[CurrentKey])
                {
                    KeysJustPressed[CurrentKey] = true;   
                }
                else if (!KeyPressed && AllKeys[CurrentKey])
                {
                    KeysJustReleased[CurrentKey] = true;
                }
                else
                {
                    KeysJustPressed[CurrentKey] = false;
                    KeysJustReleased[CurrentKey] = false;
                }

                KeysReleased[CurrentKey] = !KeyPressed;
                AllKeys[CurrentKey] = KeyPressed;
            }
        }
        
        
        public bool KeyPressed(Key DesiredKey)
        {
            return AllKeys[DesiredKey];
        }
        
        public bool KeyJustPressed(Key DesiredKey)
        {
            return KeysJustPressed[DesiredKey];
        }
        
        public bool KeyJustReleased(Key DesiredKey)
        {
            return KeysJustPressed[DesiredKey] == false && AllKeys[DesiredKey] == false;
        }
    }
}