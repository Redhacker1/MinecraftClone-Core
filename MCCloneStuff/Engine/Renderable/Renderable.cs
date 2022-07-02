using System;
using System.Numerics;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Veldrid;

namespace Engine.Renderable
{
    public abstract class Renderable
    {
        protected IndexBuffer<uint> ebo;
        protected BaseVertexBuffer vbo;

        protected MinimalObject _objectReference;

        protected internal bool UseIndexedDrawing;
        public uint VertexElements;
        public Vector3 Scale = new Vector3(1);
        public bool Render = true;
        
        public Vector3 Position => GetObjectReference().Pos;

        protected MinimalObject GetObjectReference()
        {
            return _objectReference;
        }
        public Matrix4x4 ViewMatrix => Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale)
            * Matrix4x4.CreateTranslation(GetObjectReference().Pos - Camera.MainCamera.Pos);
        
        public Quaternion Rotation => GetObjectReference().Rotation;


        /// <summary>
        /// Logic to decide if the object should be rendered, called for each item
        /// </summary>
        /// <returns>Whether the object should be rendered</returns>
        public abstract bool ShouldRender(Frustrum frustum);

        internal virtual void BindResources(CommandList list)
        {
            ebo?.Bind(list);
            vbo.Bind(list);
        }
    }
}