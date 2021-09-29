namespace Engine.Renderable
{
    public interface IRenderable
    {
        /// <summary>
        /// Logic to decide if the object should be rendered, called for each item
        /// </summary>
        /// <returns>Whether the object should be rendered</returns>
        internal bool ShouldRender()
        {
            return true;
        }

        internal void BindFlags();

        internal void BindResources();
    }
}