using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Rendering.Veldrid;
using Veldrid;

namespace Engine.Rendering.Abstract
{
    /// <summary>
    /// The default general purpose render pass for the engine, renders all models added to this pass,
    /// little else is done, but might be a good starting point for future expansion and optimization!.
    /// </summary>
    // TODO: move from 
    public class DepthPrepass : RenderPass  
    {
        VertexBuffer<Matrix4x4> Transforms;
        ResourceSet CameraResourceSet;
        CommandList TransformsUpdateList;
        
        public DepthPrepass(Renderer _backingRenderer) : base(_backingRenderer)
        {
            Name = "Depth Prepass";
            
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

            CameraResourceSet = backingRenderer.Device.ResourceFactory.CreateResourceSet(resourceSetDescription);

           TransformsUpdateList = _backingRenderer.Device.ResourceFactory.CreateCommandList();
           TransformsUpdateList.Name = "TransformsUpdatePass";


        }

        Matrix4x4[] transforms = new Matrix4x4[1];
        protected override void PrePass(ref CameraInfo camera, CommandList list, List<Instance3D> instances)
        {
            // Create the matrix 
            Span<Matrix4x4> updateMatrix = stackalloc Matrix4x4[2];
            if (camera.Self != null)
            {
                updateMatrix[0] = camera.Self.GetProjectionMatrix();
                updateMatrix[1] = camera.Self.GetViewMatrix();
                ViewProjBuffer.ModifyBuffer(updateMatrix);
            }
            
            int countNumber = instances.Count;
            if (transforms == null || countNumber > transforms.Length)
            {
                transforms = new Matrix4x4[countNumber];                
            }

            for (int index = 0; index < countNumber; index++)
            {
                Transform instanceTransform = instances[index].GetCameraSpaceTransform(ref camera);
                Transform.Compose(in instanceTransform, out Matrix4x4 outMatrix);
                transforms[index] = outMatrix;
            }
            Transforms.ModifyBuffer(transforms, backingRenderer.Device);

        }

        protected override void Pass(CommandList list, List<Instance3D> instances, ref CameraInfo info)
        {
            if (instances.Count > 0)
            {
                for (int index = 0; index < instances.Count; index++)
                {
                    Instance3D instance = instances[index];
                    if (instance.ModelMaterial.Bind(list, CameraResourceSet, true))
                    {
                        instance._baseRenderableElement.BindResources(list);
                        list.SetVertexBuffer(0, Transforms.BufferObject);

                        if (instance._baseRenderableElement.UseIndexedDrawing)
                        {
                            list.DrawIndexed(instance._baseRenderableElement.VertexElements, 1, 0, 0, (uint)index);
                        }
                        else
                        {
                            list.Draw(instance._baseRenderableElement.VertexElements, 1, 0, (uint)index);      
                        }
                    }
                }   
            }
        }
    }
}