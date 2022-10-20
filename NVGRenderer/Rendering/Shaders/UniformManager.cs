using System.Runtime.InteropServices;
using Engine.Utilities.LowLevel.Memory;

namespace NVGRenderer.Rendering.Shaders
{
    public class UniformManager
    {

        public readonly uint FragSize;

        private readonly ByteStore<FragUniforms> _uniforms;
        private int _count;
        private int _capacity;

        public int CurrentsOffset => _count;

        public ReadOnlySpan<FragUniforms> Uniforms => _uniforms.Span;

        public UniformManager(uint fragSize)
        {
            FragSize = fragSize;
            _uniforms = ByteStore<FragUniforms>.Create(NativeMemoryHeap.Instance, 10);
            _count = 0;
            _capacity = 0;
        }

        private int AllocUniforms(int n)
        {
            if (_count + n > _capacity)
            {
                uint cuniforms = (uint)(Math.Max(_count + n, 128) + (int)(_uniforms.Count / 2));
                _uniforms.EnsureCapacity(cuniforms * FragSize);
                _capacity = (int)cuniforms;
            }
            return (int)(_count * FragSize);
        }

        public unsafe int AddUniform(FragUniforms uniforms)
        {
            int ret = AllocUniforms(1);

            ReadOnlySpan<FragUniforms> bytes = new ReadOnlySpan<FragUniforms>(&uniforms, 1);
            _uniforms.AppendRange(bytes);
            _count += 1;
            return ret;
        }

        public void Clear()
        {
            _uniforms.Clear();
            _count = 0;
        }

        ~UniformManager()
        {
            _uniforms.Dispose();
        }
    }
}