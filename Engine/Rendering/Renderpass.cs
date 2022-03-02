using System.Numerics;
using Veldrid;

namespace Engine.Rendering
{
    public abstract class Renderpass
    {
        internal UniformBuffer<Matrix4x4> ViewProjBuffer;
        internal UniformBuffer<Matrix4x4> WorldBuffer;
        internal ResourceSet baseResourceSet;
        CommandList cmdList;
        protected Renderer backingRenderer;

        protected Renderpass(CommandList _list, Renderer renderer)
        {
            cmdList = _list;
            backingRenderer = renderer;
        }
        protected Renderpass(Renderer renderer)
        {
            backingRenderer = renderer;
        }
        
        /// <summary>
        ///  This is where your preperation for your pass goes.
        /// </summary>
        /// <param name="list"></param>
        protected virtual void PrePass(CommandList list)
        {
            
        }

        /// <summary>
        /// Do your cleanup and final operations in this pass.
        /// </summary>
        /// <param name="list"></param>
        protected virtual void PostPass(CommandList list)
        {
            
        }

        /// <summary>
        /// This is where you actually do your pass related logic, updating camera position, filtering out some objects, etc.
        /// </summary>
        /// <param name="list"></param>
        protected abstract void Pass(CommandList list);


        internal void RunPass()
        {
            if (cmdList != null)
            {
                RunPass(cmdList);   
            }
        }

        internal void RunPass(CommandList list)
        {
            PrePass(list);
            Pass(list);
            PostPass(list);
        }



    }
}