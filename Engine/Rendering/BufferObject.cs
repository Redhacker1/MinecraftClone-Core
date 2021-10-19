using System;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;

namespace Engine.Rendering
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private uint _handle;
        private BufferTargetARB _bufferType;
        private GL _gl;

        public readonly uint Length;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            _gl = gl;
            _bufferType = bufferType;

            _handle = _gl.GenBuffer();
            Bind();
            Length = (uint) data.Length;
            fixed (void* d = data)
            {
                _gl.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public void Bind()
        {
            _gl.BindBuffer(_bufferType, _handle);
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(_handle);
        }

        public void ModifyBuffer(Span<TDataType> data)
        {
            _gl.BindBuffer(GLEnum.Buffer, _handle);
            int datatypesize = 0;
            unsafe
            {
                datatypesize = sizeof(TDataType);
            }

            uint potential_size = (uint)(data.Length * datatypesize);

            if (Length * datatypesize >= data.Length * datatypesize)
            {
                _gl.BufferSubData<TDataType>(_bufferType,0, data);
            }
            else
            {
                _gl.BufferData<TDataType>(_bufferType, data ,BufferUsageARB.StaticDraw);
            }
        }
    }
}
