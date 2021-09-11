using System;
using System.Numerics;
using Engine.Renderable;
using Engine.Windowing;
using Silk.NET.OpenGL;

namespace Engine.Rendering
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private uint _handle;
        public readonly uint Vertexcount;
        private GL _gl = WindowClass.GlHandle;
        Mesh meshinstance;

        /// <summary>
        /// If VAO is properly bound this will be true.
        /// </summary>
        public bool validVAO = false;

        public Matrix4x4 ModelMatrix
        {
            get =>
                Matrix4x4.CreateFromQuaternion(meshinstance.Rotation) * Matrix4x4.CreateScale(meshinstance.Scale) *
                Matrix4x4.CreateTranslation(meshinstance.Position);

        }

        BufferObject<TVertexType> vertexBufferObject;
        BufferObject<TIndexType> indexBufferObject;

        

        public VertexArrayObject(BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
        {

            _handle = _gl.GenVertexArray();
            Console.WriteLine(_handle);
            Bind();

            vertexBufferObject = vbo;
            vbo.Bind();
            indexBufferObject = ebo;
            ebo.Bind();
            Unbind();
            validVAO = true;
            
            Vertexcount = vbo.Length;

        }
        
        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo, Mesh mesh)
        {
            _gl = gl;
            
            _handle = _gl.GenVertexArray();
            Console.WriteLine(_handle);
            Bind();
            vbo.Bind();
            ebo.Bind();
            Unbind();
            validVAO = true;

            meshinstance = mesh;
            Vertexcount = vbo.Length;
            
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
        
        public void Unbind()
        {
            _gl.DisableVertexAttribArray(_handle);
        }

        
        public void Dispose()
        {
            validVAO = false;
            _gl.DeleteVertexArray(_handle);
            indexBufferObject.Dispose();
            vertexBufferObject.Dispose();

        }
        
        
    }
}