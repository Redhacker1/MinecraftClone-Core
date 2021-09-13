namespace Engine.Input
{
    public abstract class InputDevice
    {
        protected string DeviceName;
        protected int ID;

        public abstract void Poll();

    }
}