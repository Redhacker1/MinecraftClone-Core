using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Rendering.Veldrid;
using Veldrid;

namespace Engine.Rendering.Abstract
{
    // QUESTION: What does a good API for using this even look like?
    // TODO: This is specialized for 3D, the 3D pass mesh code should be moved out and turned into a specialized class for that and the 2D code should be made into a separate renderpass arch
    public abstract class RenderPass
    {
        internal UniformBuffer<Matrix4x4> ViewProjBuffer;
        CommandList cmdList;
        protected Renderer backingRenderer;
        
        List<WeakReference<Instance3D>> Instances = new List<WeakReference<Instance3D>>();

        protected RenderPass(CommandList _list, Renderer renderer)
        {
            cmdList = _list;
            backingRenderer = renderer;
        }
        protected RenderPass(Renderer renderer)
        {
            backingRenderer = renderer;
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
        protected abstract void Pass(CommandList list, List<Instance3D> instances, ref CameraInfo camera);

        internal void RunPass()
        {
            if (cmdList != null)
            {
                RunPass(cmdList);   
            }
        }

        internal virtual void RunPass(CommandList list)
        {
            CameraInfo cameraInfo = default;
            List<Instance3D> instances;
            
            if (Camera.MainCamera != null)
            {
                cameraInfo = new CameraInfo(Camera.MainCamera);   
            }
            lock (Instances)
            {
                instances = Cull(Instances, ref cameraInfo);
            }
            Sort(instances, cameraInfo);
            PrePass(ref cameraInfo, list, instances);
            Pass(list, instances, ref cameraInfo);
            PostPass(list);
        }
        
        protected virtual List<Instance3D> Cull(List<WeakReference<Instance3D>> instances, ref CameraInfo cameraInfo)
        {
            List<Instance3D> instance3Ds = new List<Instance3D>(instances.Count);
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
        
        int GreaterThan(Vector3 first, Vector3 second, ref CameraInfo cameraInfo)
        {
            if (Vector3.Distance(cameraInfo.CameraPos, first) > Vector3.Distance(cameraInfo.CameraPos, second))
            {
                return 1;
            }
            if (Math.Abs(Vector3.Distance(cameraInfo.CameraPos, first) - Vector3.Distance(cameraInfo.CameraPos, second)) < float.Epsilon)
            {
                return 0;
            }
            if (Vector3.Distance(cameraInfo.CameraPos, first) < Vector3.Distance(cameraInfo.CameraPos, second))
            {
                return -1;
            }
            // Only should happen if a value does not possibly equal itself, IE, pretty much only NAN
            throw new ArithmeticException("Input was probably NAN, check your code");
        }
        
        public void AddInstance(Instance3D instance)
        {
            lock (Instances)
            {
                Instances.Add(new WeakReference<Instance3D>(instance));   
            }
        }
        
        public void RemoveInstance(Instance3D instance)
        {
            lock (Instances)
            {
                Instances.Remove(new WeakReference<Instance3D>(instance));
            }
            
        }
    }
}