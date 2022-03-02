using Engine.Rendering;
using Engine.Rendering.Culling;
using Veldrid;

namespace Engine.Renderable
{
    public abstract class Renderable
    {
        protected IndexBuffer<uint> ebo;
        protected BaseVertexBuffer vbo;

        /// <summary>
        /// Logic to decide if the object should be rendered, called for each item
        /// </summary>
        /// <returns>Whether the object should be rendered</returns>
        internal abstract bool ShouldRender(Frustrum frustum);

        internal abstract void BindResources(CommandList list);

        internal void Draw()
        {

        }
    }
}