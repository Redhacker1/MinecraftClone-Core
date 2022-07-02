using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Rendering.Veldrid;
using Veldrid;

namespace Engine.Rendering.Abstract
{
    /// <summary>
    /// The default general purpose render pass for the engine, renders all materials added to this pass,
    /// little else is done, but might be a good starting point for future expansion and mimics current functionality.
    /// </summary>
    // TODO: move from 
    public class DefaultRenderPass : RenderPass
    {
        VertexBuffer<Matrix4x4> Transforms;
        CommandList TransformsUpdateList;
        
        public DefaultRenderPass(Renderer _backingRenderer) : base(_backingRenderer)
        {
            backingRenderer = _backingRenderer;
            ViewProjBuffer = new UniformBuffer<Matrix4x4>(_backingRenderer.Device, 2);
            ViewProjBuffer.bufferObject.Name = "ViewProjBuffer";

            Transforms = new VertexBuffer<Matrix4x4>(backingRenderer.Device, Span<Matrix4x4>.Empty);


            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer,
                    ShaderStages.Vertex));
            ResourceLayout layout = _backingRenderer.Device.ResourceFactory.CreateResourceLayout(resourceLayoutDescription);
            ResourceSetDescription resourceSetDescription =
                new ResourceSetDescription(layout, ViewProjBuffer.bufferObject);

           TransformsUpdateList = _backingRenderer.Device.ResourceFactory.CreateCommandList();
           TransformsUpdateList.Name = "TransformsUpdatePass";


        }
        protected override void PrePass(ref CameraInfo camera, CommandList list, List<Instance3D> instances)
        {
            // Create the matrix 
            Span<Matrix4x4> updateMatrix = stackalloc Matrix4x4[2];
            if (camera.Self != null)
            {
                updateMatrix[0] = camera.Self.GetProjectionMatrix();
                updateMatrix[1] = camera.Self.GetViewMatrix();
                ViewProjBuffer.ModifyBuffer(updateMatrix, backingRenderer.Device);
            }

// A more memory efficent, though possibly slower version 
#if EfficentTransformsExperimental

            // TODO: after modifying the buffers to use a single base class and adding the ability to offset, this can be done in batches using only the stack.
            Span<Matrix4x4> transforms = stackalloc Matrix4x4[512];
            (int, int) passes = Math.DivRem(instances.Count, 512);
            for (int passCount = 0; passCount == passes.Item1 ; passCount++)
            {
                int countNumber = passCount == passes.Item1 ? passes.Item2 : 512;
                
                int offset = passCount * 512;
                for (int index = 0; index < countNumber; index++)
                {
                    transforms[index] = instances[passCount + offset].GetCameraSpacePos(camera.Self);
                }
                Transforms.ModifyBuffer(transforms, backingRenderer.Device, (uint)passCount * 512);
            }
#else
            // Possibly faster, though consumes quite a bit more memory, could be cached though as to improve memory usage
            // Alternatively we could look into stackalloc, though it would be more dangerous
            int countNumber = instances.Count;
            Matrix4x4[] transforms = new Matrix4x4[countNumber];
            for (int index = 0; index < countNumber; index++)
            {
                transforms[index] = instances[index].GetCameraSpacePos(camera.Self);
            }
            Transforms.ModifyBuffer(transforms, backingRenderer.Device);
#endif




        }

        protected override void Pass(CommandList list, List<Instance3D> instances, ref CameraInfo info)
        {
            if (instances.Count > 0)
            {
                for (int index = 0; index < instances.Count; index++)
                {
                    Instance3D instance = instances[index];
                    instance.ModelMaterial.Bind(list);
                
                
                    instance._baseRenderableElement.BindResources(list);
                    list.Draw(instance._baseRenderableElement.VertexElements, 1, 0, (uint)index);
                }   
            }
        }
    }
}