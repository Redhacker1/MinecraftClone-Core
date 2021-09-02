using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Engine.Rendering
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private uint _handle;
        public readonly uint Vertexcount;
        private GL _gl;
        Mesh meshinstance;

        public Matrix4x4 ModelMatrix =>
            Matrix4x4.CreateFromQuaternion(meshinstance.Rotation) * Matrix4x4.CreateScale(meshinstance.Scale) *
            Matrix4x4.CreateTranslation(meshinstance.Position);
        
        public AABB CullingBox;

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo, AABB CullingMesh, Mesh boundmesh)
        {
            _gl = gl;

            _handle = _gl.GenVertexArray();
            vbo.Bind();
            ebo.Bind();

            Vertexcount = vbo.Length;
            

            this.meshinstance = boundmesh;
            this.CullingBox = CullingMesh;
        }
        

        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
        {
            _gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint) sizeof(TVertexType), (void*) (offSet * sizeof(TVertexType)));
            _gl.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            _gl.BindVertexArray(_handle);
        }

        
        public void Dispose()
        {
            _gl.DeleteVertexArray(_handle);
        }
        
        
    }
}