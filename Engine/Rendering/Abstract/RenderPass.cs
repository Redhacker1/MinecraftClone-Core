using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Objects.SceneSystem;
using Engine.Rendering.Abstract.View;
using Engine.Rendering.VeldridBackend;
using Engine.Utilities.Concurrency;
using Veldrid;
using RenderState = Engine.Rendering.Abstract.RenderStage.RenderState;

namespace Engine.Rendering.Abstract
{
    // QUESTION: What does a good API for using this even look like?
    // TODO: This is specialized for 3D, the 3D pass mesh code should be moved out and turned into a specialized class for that and the 2D code should be made into a separate renderpass arch
    /// <summary>
    /// OBSOLETE: use <see cref="RenderStage"/> instead, This class will likely be re-adapted at a later time. 
    /// </summary>
    [Obsolete("Being replaced by the newer RenderStage system")]
    public abstract class RenderPass : RenderStage.RenderStage
    {
        internal UniformBuffer<Matrix4x4> ViewProjBuffer;
        protected Renderer BackingRenderer;
        public string Name;
        public bool FrustumCull = true;
        
        ThreadSafeList<WeakReference<Instance3D>> Instances = new ThreadSafeList<WeakReference<Instance3D>>();

        protected RenderPass(CommandList _list, Renderer renderer, string name = null )
        {
            Name = name;
            BackingRenderer = renderer;
        }
        protected RenderPass(Renderer renderer, string name = null )
        {
            Name = name;
            BackingRenderer = renderer;
        }

        /// <summary>
        ///  This is where your preparation for your pass goes.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="list"></param>
        /// <param name="instances"></param>
        protected virtual void PrePass(ref CameraInfo camera, CommandList list, IReadOnlyList<Instance> instances)
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
        /// <param name="list">The target command list to execute</param>
        /// <param name="instances">The sorted list of Instances to draw</param>
        /// <param name="camera">"Information about the camera being used"</param>
        protected abstract void Pass(CommandList list, IReadOnlyList<Instance> instances, ref CameraInfo camera);
        
        protected override void Stage(RenderState rendererState, RenderTarget targetFrame, float _, float _1,
            IReadOnlyList<Instance> renderObjects)
        {
            if (Camera.MainCamera == null) return;

            CommandList list = rendererState.GlobalCommandList;
            list.SetFramebuffer(targetFrame.Framebuffer);
            
            CameraInfo cameraInfo = new CameraInfo(Camera.MainCamera);


            if (Name != null)
            {
                list.PushDebugGroup(Name);      
            }

            PrePass(ref cameraInfo, list, renderObjects);
            Pass(list, renderObjects, ref cameraInfo);
            PostPass(list);
            if (Name != null)
            {
                list.PopDebugGroup();   
            }
        }
    }
}