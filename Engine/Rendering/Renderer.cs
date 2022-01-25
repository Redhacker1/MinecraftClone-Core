using System;
using System.Numerics;
using Engine.Renderable;
using Engine.Rendering.Shared.Culling;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using Veldrid;


struct ViewProj
{
    public Matrix4x4 view;
    public Matrix4x4 Projection;

}
namespace Engine.Rendering
{
    
    /// <summary>
    /// Rendering Logic goes here, potentially could be the base for other rendering backends.
    /// </summary>
    public class Renderer
    {
        public DeviceBuffer ProjectionBuffer;
        public DeviceBuffer ViewBuffer;

        public DeviceBuffer WorldBuffer;
        private ViewProj WorldDataBuffer;
        internal Renderer(IView viewport)
        {
            unsafe
            {
                Device = viewport.CreateGraphicsDevice(GraphicsBackend.Vulkan);
                List = Device.ResourceFactory.CreateCommandList();
                ProjectionBuffer = Device.ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                ViewBuffer = Device.ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                WorldBuffer = Device.ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            }
        }

        public GraphicsDevice Device;
        private CommandList List; 
        internal void OnRender(double time)
        {
            if (Camera.MainCamera != null)
            {
                Device.UpdateBuffer(ProjectionBuffer, 0 , Camera.MainCamera.GetProjectionMatrix());
                Device.UpdateBuffer(ViewBuffer, 0 , Camera.MainCamera.GetViewMatrix());
            }
            Frustrum? frustum =  Camera.MainCamera?.GetViewFrustum();
            List.Begin();
            //Console.WriteLine("New frame");
            //Console.WriteLine("Frame Begin!");
            List.SetFramebuffer(Device.SwapchainFramebuffer);
            List.ClearColorTarget(0, RgbaFloat.White);
            //List.ClearDepthStencil(1f);

            Mesh[] sceneData = Mesh.Meshes.ToArray();
            //Console.WriteLine(sceneData.Length);
            foreach (Mesh mesh in sceneData)
            {

                if (IntersectionHandler.MeshInFrustrum(mesh, frustum))
                {
                    if(mesh.BindResources(List))
                    {
                        List.UpdateBuffer(WorldBuffer, 0, mesh.ViewMatrix);
                        List.Draw(mesh.VertexElements);
                    }
                    else
                    {
                        Console.WriteLine("Mesh failed to bind!");
                    }
                    /*if (IntersectionHandler.MeshInFrustrum(mesh, ref frustum) && mesh?.ActiveState == MeshState.Render)
                    {
                    }*/                   
                }
 
            }

            List.End();
            Device.SubmitCommands(List);
            Device.SwapBuffers();
            Device.WaitForIdle();
            
        }
        
        internal void OnResize(Vector2D<int> size)
        {
            // Adjust the viewport to the new window size
            Device.ResizeMainWindow((uint)size.X,(uint)size.Y);
        }
        /*
        void OnRender(double time)
        {
            Frustrum? frustum =  Camera.MainCamera?.GetViewFrustum();

            GlHandle.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            GlHandle.Enable(EnableCap.DepthTest);
            GlHandle.Enable(EnableCap.CullFace);
            GlHandle.Enable(EnableCap.DebugOutput);
            GlHandle.DepthFunc(DepthFunction.Lequal);


            
            if (Camera.MainCamera != null)
            {
                Shader?.SetUniform("uView", Camera.MainCamera.GetViewMatrix());
                Shader?.SetUniform("uProjection", Camera.MainCamera.GetProjectionMatrix());
            }

            int MeshesDrawn = 0;
            Mesh mesh;
            for (int meshindex = 0; meshindex < Mesh.Meshes.Count; meshindex++)
            {
                mesh = Mesh.Meshes[meshindex];
                if (mesh != null)
                {
                    if (mesh.ActiveState == MeshState.Delete)
                    {
                        mesh.Dispose();
                        Mesh.Meshes.Remove(mesh);
                    }
                    else if (mesh.ActiveState == MeshState.Dirty)
                    {
                        mesh.MeshReference?.Dispose();
                        mesh.MeshReference = mesh.RegenerateVao();
                        mesh.ActiveState = MeshState.Render;
                    }
                    if(mesh.ActiveState == MeshState.Render)
                    {


                        if (mesh?.MeshReference != null && mesh.ActiveState == MeshState.Render && IntersectionHandler.MeshInFrustrum(mesh, ref frustum))
                        {
                            
                            GlHandle.CullFace(CullFaceMode.Front);
                            Texture?.Bind();
                            Shader?.Use();
                            
                            mesh.BindBuffers();
                            Shader?.SetUniform("uModel", mesh.ViewMatrix);
                            //Shader?.SetUniform("uTexture0", 0);
                            mesh.Draw(c);
                        }
                        else
                        {
                            MeshesDrawn += 1;
                        }
                    }
                    
                }
            }

            //TODO: Layered UI/PostProcess

            // TODO: Re-enable IMGUI code when everything is working properly.
            /*
            foreach (ImGUIPanel uiPanel in ImGUIPanel.panels)
            {
                ImGui.Begin(uiPanel.PanelName);
                uiPanel.CreateUI();
                ImGui.End();
            }
            GuiController.Render();
            
        }
        */
    }
}