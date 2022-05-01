using System;
using System.Numerics;
using System.Security.AccessControl;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Engine.Windowing;
using Veldrid;

namespace Engine.Renderable
{
    public abstract class Renderable
    {
        protected IndexBuffer<uint> ebo;
        protected BaseVertexBuffer vbo;
        public UniformBuffer<Matrix4x4> WorldBuffer;
        internal ResourceSet WorldBufferSet;

        protected MinimalObject _objectReference;
        
        public bool UseIndexedDrawing;
        public uint VertexElements;

        private  Vector3 internal_Position = Vector3.Zero;

        public Vector3 Position
        {
            get => internal_Position;
            set
            {
                var modifybuffer = new Span<Matrix4x4>();
                modifybuffer[0] = ViewMatrix;
                internal_Position = value;
                WorldBuffer.ModifyBuffer(modifybuffer);
            }
        }
        
        private  Vector3 internal_Scale = Vector3.One;

        public Vector3 Scale
        {
            get => internal_Scale;
            set
            {
                var modifybuffer = new Span<Matrix4x4>();
                modifybuffer[0] = ViewMatrix;
                internal_Scale = value;
                WorldBuffer.ModifyBuffer(modifybuffer);
            }
        }
        
        
        private Quaternion internal_Rotation = Quaternion.Identity;

        public Quaternion Rotation
        {
            get =>  internal_Rotation;
            set
            {
                var modifybuffer = new Span<Matrix4x4>();
                modifybuffer[0] = ViewMatrix;
                internal_Rotation = value;
                WorldBuffer.ModifyBuffer(modifybuffer);
            }
        }

        public Matrix4x4 ViewMatrix => Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale)
                                                                        * Matrix4x4.CreateTranslation(Position -Camera.MainCamera.Pos);


        /// <summary>
        /// Logic to decide if the object should be rendered, called for each item
        /// </summary>
        /// <returns>Whether the object should be rendered</returns>
        public abstract bool ShouldRender(Frustrum frustum);


        public Renderable()
        {
            // This is a mess, clean it up!
            ResourceLayoutDescription PerMeshData = new ResourceLayoutDescription(new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));
            var layout = WindowClass._renderer.Device.ResourceFactory.CreateResourceLayout(PerMeshData);
            WorldBuffer = new UniformBuffer<Matrix4x4>(WindowClass._renderer.Device, 1, "Mesh Worldbuffer");
            WorldBufferSet = WindowClass._renderer.Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout, WorldBuffer.bufferObject));
        }

        internal virtual void BindResources(CommandList list)
        {
            ebo?.Bind(list);
            vbo.Bind(list);
        }
    }
}