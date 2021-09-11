using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Rendering;
using Silk.NET.Input;

namespace Engine.Input
{
    public class InputHandler
    {
        internal static IKeyboard KeyboardHandle;
        public static IMouse MouseHandle;

        static List<IInputDevice> AllInputDevices = new List<IInputDevice>();

        static Dictionary<Key, bool> AllKeys = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> KeysJustPressed = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> KeysJustReleased = new Dictionary<Key, bool>();
        
        static Dictionary<Button, bool> AllButtons = new Dictionary<Button, bool>();
        static Dictionary<Button, bool> ButtonJustPressed = new Dictionary<Button, bool>();
        static Dictionary<Button, bool> ButtonJustReleased = new Dictionary<Button, bool>();
        
        
        static Dictionary<MouseButton, bool> AllMouseKeys = new Dictionary<MouseButton, bool>();
        static Dictionary<MouseButton, bool> MouseKeysJustPressed = new Dictionary<MouseButton, bool>();
        static Dictionary<MouseButton, bool> MouseKeysJustReleased = new Dictionary<MouseButton, bool>();

        public static void initKeyboardHandler(IInputContext inputContext)
        {
            AllInputDevices.AddRange(inputContext.Joysticks);
            AllInputDevices.AddRange(inputContext.Mice);
            AllInputDevices.AddRange(inputContext.OtherDevices);

            foreach (IGamepad Gamepad in inputContext.Gamepads)
            {
                foreach (Button button in Gamepad.Buttons)
                {
                    bool ButtonPressed = button.Pressed;
                    AllButtons[button] = ButtonPressed;
                    ButtonJustPressed[button] =  ButtonPressed;
                    ButtonJustPressed[button] = ButtonPressed;
                }
                
                
            }
            
            
            foreach (IKeyboard Keyboard in inputContext.Keyboards)
            {
                foreach (Key Keys in Keyboard.SupportedKeys)
                {
                    bool KeyPressed = KeyboardHandle.IsKeyPressed(Keys);
                    AllKeys[Keys] = KeyPressed;
                    KeysJustPressed[Keys] =  KeyPressed;
                    KeysJustReleased[Keys] = KeyPressed;
                }
            }
            
            foreach (IMouse Mouse in inputContext.Mice)
            {
                Mouse.IsButtonPressed(0);
                Mouse.Cursor.CursorMode = CursorMode.Raw;
                Mouse.MouseMove += OnMouseMove;
                foreach (MouseButton mouseButtons in Mouse.SupportedButtons)
                {
                    Console.WriteLine(mouseButtons);
                    bool KeyPressed = Mouse.IsButtonPressed(mouseButtons);
                    AllMouseKeys[mouseButtons] = KeyPressed;
                    MouseKeysJustPressed[mouseButtons] =  KeyPressed;
                    MouseKeysJustReleased[mouseButtons] = KeyPressed;
                }
                
                
            }
            

        }

        public static void PollKeyboard()
        {
            foreach (Key CurrentKey in AllKeys.Keys)
            {
                bool KeyPressed = KeyboardHandle.IsKeyPressed(CurrentKey);

                if (KeysJustPressed[CurrentKey] == false && KeyPressed == true)
                {
                    KeysJustPressed[CurrentKey] = true;   
                }
                else if (KeyPressed == false && AllKeys[CurrentKey])
                {
                    KeysJustReleased[CurrentKey] = false;
                }
                else
                {
                    KeysJustPressed[CurrentKey] = false;   
                }
                AllKeys[CurrentKey] = KeyPressed;
            }
        }

        public static bool KeyPressed(Key DesiredKey)
        {
            return AllKeys[DesiredKey];
        }
        
        public static bool KeyJustPressed(Key DesiredKey)
        {
            return KeysJustPressed[DesiredKey];
        }
        
        public static bool KeyJustReleased(Key DesiredKey)
        {
            return KeysJustPressed[DesiredKey] == false && AllKeys[DesiredKey] == false;
        }
        
        
        private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            float lookSensitivity = 0.1f;
            if (LastMousePosition == default) { LastMousePosition = position; }
            else
            {
                float xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
                float yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
                LastMousePosition = position;

                if (Camera.MainCamera != null)
                {
                    Camera.MainCamera.Yaw += xOffset;
                    Camera.MainCamera.Pitch -= yOffset;
                    
                    //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
                    Camera.MainCamera.Pitch = Math.Clamp(Camera.MainCamera.Pitch, -89.0f, 89.0f);
                
                    Vector3 CameraDirection = Vector3.Zero;
                    CameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                    CameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                    CameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Camera.MainCamera.Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Camera.MainCamera.Pitch));
                    Camera.MainCamera.Front = Vector3.Normalize(CameraDirection);
                }
                
            }
        }

        public static Vector2 LastMousePosition { get; set; }

        Action<InputDevice, Key> Keyboard_Onkeydown;
    }
}