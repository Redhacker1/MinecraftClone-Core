namespace Engine.Renderable
{
    public abstract class Renderable
    {
        /// <summary>
        /// Logic to decide if the object should be rendered, called for each item
        /// </summary>
        /// <returns>Whether the object should be rendered</returns>
        internal bool ShouldRender()
        {
            return true;
        }

        abstract internal void BindFlags();

        abstract internal void BindResources();
    }
}