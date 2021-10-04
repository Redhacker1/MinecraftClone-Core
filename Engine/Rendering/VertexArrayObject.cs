using Silk.NET.OpenGL;

namespace Engine.Rendering
{
    public class VertexArrayObject<TVertexType, TIndexType> 
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        public uint _handle;
        private GL _gl;
        BufferObject<TVertexType> vbo_p;
        BufferObject<TIndexType> ebo_p;

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
        {
            _gl = gl;
            ebo_p = ebo;
            vbo_p = vbo;

            _handle = _gl.GenVertexArray();
            Bind();
            ebo.Bind();
            vbo.Bind();
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
            vbo_p.Dispose();
            ebo_p.Dispose();
        }
    }
}