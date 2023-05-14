using Engine.Utilities.MathLib;

namespace Engine.Windowing;

public abstract class HighLevelInputAbstraction
{
    public MouseState OnMouseMoved;

    public abstract void OnKeyPressed(Keycode key, KeyModifiers modifiers, char keyChar);

    //protected Int2 MouseDelta();


}