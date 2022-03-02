using System;
using System.Numerics;
using Engine.Rendering.Culling;
using Veldrid;
using Plane = Engine.Rendering.Culling.Plane;

namespace Engine.Rendering
{
    /// <summary>
    /// The default general purpose render pass for the engine, renders all materials added to this pass,
    /// little else is done, but might be a good starting point for future expansion and mimics current functionality.
    /// </summary>
    public class BasicMaterialRenderPass : Renderpass
    {


        ThreadSafeList<Material> MaterialsInPass = new ThreadSafeList<Material>();
        Camera _camera => Camera.MainCamera;

        Frustrum Frustrum;
        
        static readonly ResourceLayoutDescription ViewProjLayout = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer,
                ShaderStages.Vertex)
        );
        static readonly ResourceLayoutDescription ViewModelLayout = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer,
                ShaderStages.Vertex)
        );

        public BasicMaterialRenderPass(Camera camera, Renderer _backingRenderer) : base(_backingRenderer)
        {
            //_camera = camera;
            backingRenderer = _backingRenderer;
            ViewProjBuffer = new UniformBuffer<Matrix4x4>(_backingRenderer.Device, 2);
            WorldBuffer = new UniformBuffer<Matrix4x4>(_backingRenderer.Device, 1);
            ViewProjBuffer.bufferObject.Name = "ViewProjBuffer";
            
            
            var layout = _backingRenderer.Device.ResourceFactory.CreateResourceLayout(ViewProjLayout);
            
            ResourceSetDescription ResourceSetDesc = new ResourceSetDescription(layout, ViewProjBuffer.bufferObject);
            
            baseResourceSet =
                _backingRenderer.Device.ResourceFactory.CreateResourceSet(ResourceSetDesc);
        }


        protected override void PrePass(CommandList list)
        {
            Span<Matrix4x4> UpdateMatrix = stackalloc Matrix4x4[2];
            Span<Plane> sides = stackalloc Plane[6];

            if (_camera != null)
            {
                Frustrum = _camera.GetViewFrustum(sides);
                UpdateMatrix[0] = _camera.GetProjectionMatrix();

                UpdateMatrix[1] = _camera.GetViewMatrix();
                ViewProjBuffer.ModifyBuffer(UpdateMatrix, backingRenderer.Device);
            }

        }

        protected override void Pass(CommandList list)
        {
            
            if(_camera == null)
                return;
            MaterialsInPass = Material._materials;
            MaterialsInPass.EnterReadLock();
            foreach (Material Material in MaterialsInPass)
            {
                Material.Render(list, Frustrum, backingRenderer);
            }
            MaterialsInPass.ExitReadLock();
        }
    }
}