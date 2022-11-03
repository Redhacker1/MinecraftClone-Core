using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Rendering.Abstract.View;
using Engine.Rendering.VeldridBackend;
using Engine.Utilities.Concurrency;
using Veldrid;

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
        CommandList cmdList;
        protected Renderer backingRenderer;
        public string Name;
        
        ThreadSafeList<WeakReference<Instance3D>> Instances = new ThreadSafeList<WeakReference<Instance3D>>();

        protected RenderPass(CommandList _list, Renderer renderer, string name = null )
        {
            Name = name;
            cmdList = _list;
            backingRenderer = renderer;
        }
        protected RenderPass(Renderer renderer, string name = null )
        {
            Name = name;
            backingRenderer = renderer;
            cmdList = renderer.Device.ResourceFactory.CreateCommandList();
        }

        /// <summary>
        ///  This is where your preparation for your pass goes.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="list"></param>
        protected virtual void PrePass(ref CameraInfo camera, CommandList list, List<Instance3D> instances)
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
        protected abstract void Pass(CommandList list, List<Instance3D> instances, ref CameraInfo camera);
        
        protected virtual List<Instance3D> Cull(ThreadSafeList<WeakReference<Instance3D>> instances, ref CameraInfo cameraInfo) 
        {
            List<Instance3D> instance3Ds = new List<Instance3D>(0);

            for (int i = 0; i < instances.Count; i++)
            {
                // Check if the WeakReference is valid and if the mesh it points to has not been disposed. 
                if (instances[i].TryGetTarget(out Instance3D currentInstance) && currentInstance._baseRenderableElement.Disposed == false )
                {
                    if (currentInstance.ShouldRender(ref cameraInfo) && currentInstance._baseRenderableElement.VertexElements > 0)
                    {
                        instance3Ds.Add(currentInstance);   
                    }
                }
                else
                {
                    // This should quickly remove the invalid instance without having a whole shuffle and copy of every mesh
                    // The trade-off being the list gets less organized.
                    instances[i] = instances[^1];
                    instances.RemoveAt(instances.Count - 1);
                }
            }
            return instance3Ds;
        }

        /// <summary>
        ///  The sorting algorithm, by default sorts position in world space using <see cref="List{T}.Sort()"/>>.
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="cameraInfo">Camera reference</param>
        protected virtual void Sort(List<Instance3D> instances, CameraInfo cameraInfo)
        {
            instances.Sort((instance3D, instance3D1) => GreaterThan(instance3D.Position, instance3D1.Position, ref cameraInfo));
        }

        static int GreaterThan(Vector3 first, Vector3 second, ref CameraInfo cameraInfo)
        {
            if (Vector3.Distance(cameraInfo.CameraTransform.Position, first) > Vector3.Distance(cameraInfo.CameraTransform.Position, second))
            {
                return 1;
            }
            if (Math.Abs(Vector3.Distance(cameraInfo.CameraTransform.Position, first) - Vector3.Distance(cameraInfo.CameraTransform.Position, second)) < float.Epsilon)
            {
                return 0;
            }
            if (Vector3.Distance(cameraInfo.CameraTransform.Position, first) < Vector3.Distance(cameraInfo.CameraTransform.Position, second))
            {
                return -1;
            }
            // Only should happen if a value does not possibly equal itself, IE, pretty much only NaN
            throw new ArithmeticException("Input was probably NAN, check your inputs!");
        }
        
        public void AddInstance(Instance3D instance)
        {
            Instances.Add(new WeakReference<Instance3D>(instance));   
        }
        
        public void RemoveInstance(Instance3D instance)
        {
            Instances.Remove(new WeakReference<Instance3D>(instance));
            
        }

        protected override void Stage(RenderStage.RenderState rendererState, RenderTarget targetFrame, float _, float _1)
        {
            if (Camera.MainCamera == null) return;

            CommandList list = rendererState.GlobalCommandList;
            CameraInfo cameraInfo = new CameraInfo(Camera.MainCamera);
            List<Instance3D> instances = Cull(Instances, ref cameraInfo);
            Sort(instances, cameraInfo);

            list.PushDebugGroup(Name);   
            
            PrePass(ref cameraInfo, list, instances);
            Pass(list, instances, ref cameraInfo);
            PostPass(list);
            if (Name != null)
            {
                list.PopDebugGroup();   
            }
        }
    }
}