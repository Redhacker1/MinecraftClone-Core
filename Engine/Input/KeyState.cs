namespace Engine.Input;

public enum KeyState
{
    Released = 0b00,
    JustPressed = 0b01,
    Pressed = 0b10,
    CTRLpressed = 0b100,
    ShiftPressed = 0b1000,
}