namespace Engine.Input
{
    public abstract class InputDevice
    {
        protected string DeviceName;
        protected int Id;

        public abstract void Poll();

    }
}