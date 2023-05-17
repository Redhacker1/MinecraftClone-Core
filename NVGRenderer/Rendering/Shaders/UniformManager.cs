using System.Runtime.CompilerServices;
using Engine.Utilities.LowLevel.Memory;

namespace NVGRenderer.Rendering.Shaders
{
    public unsafe class UniformManager
    {
        public readonly uint Alignment;

        private byte* _uniformHead;
        private byte* _uniformPtr;
        private nuint _byteCapacity;

        public uint CurrentOffset => (uint)(_uniformPtr - _uniformHead);

        public Span<byte> AsSpan()
        {
            return new Span<byte>(_uniformHead, (int)(_uniformPtr - _uniformHead));
        }

        public UniformManager(uint alignment)
        {
            Alignment = alignment;
            _uniformHead = (byte*)NativeMemoryHeap.Instance.Alloc(16 * Alignment, out _byteCapacity);
            Clear();
        }

        private byte* AllocUniforms(uint n)
        {
            uint request = n * Alignment;

            if ((nuint)(_uniformPtr - _uniformHead) + request > _byteCapacity)
            {
                uint prevOffset = CurrentOffset;
                nuint newCapacity = _byteCapacity * 2;

                _uniformHead = (byte*)NativeMemoryHeap.Instance.Realloc(
                    (IntPtr)_uniformHead, _byteCapacity, newCapacity, out _byteCapacity);

                _uniformPtr = _uniformHead + prevOffset;
            }

            byte* prevPtr = _uniformPtr;
            _uniformPtr += request;
            return prevPtr;
        }

        public int AddUniform(FragUniforms uniforms)
        {
            byte* ptr = AllocUniforms(1);

            Unsafe.WriteUnaligned(ptr, uniforms);

            return (int)(ptr - _uniformHead);
        }

        public void Clear()
        {
            _uniformPtr = _uniformHead;
        }

        ~UniformManager()
        {
            NativeMemoryHeap.Instance.Free(_byteCapacity, (IntPtr)_uniformHead);
        }
    }
}