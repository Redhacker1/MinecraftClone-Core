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
        
        public bool UseIndexedDrawing;
        public uint VertexElements;
        public float Scale = 1;

        protected MinimalObject GetObjectReference()
        {
            return _objectReference;
        }
        public Matrix4x4 ViewMatrix => Matrix4x4.CreateFromQuaternion(
                Quaternion.CreateFromYawPitchRoll(MathHelper.DegreesToRadians(GetObjectReference().Rotation.X), 
                    MathHelper.DegreesToRadians(GetObjectReference().Rotation.Y), 
                    MathHelper.DegreesToRadians(GetObjectReference().Rotation.Z))) * Matrix4x4.CreateScale(Scale)
            * Matrix4x4.CreateTranslation(GetObjectReference().Pos -Camera.MainCamera.Pos);
        
        public Quaternion Rotation =>
            Quaternion.CreateFromYawPitchRoll(MathHelper.DegreesToRadians(GetObjectReference().Rotation.X),
                MathHelper.DegreesToRadians(GetObjectReference().Rotation.Y),
                MathHelper.DegreesToRadians(GetObjectReference().Rotation.Z));


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