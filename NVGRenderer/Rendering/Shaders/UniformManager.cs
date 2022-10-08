﻿using System;
using System.Runtime.InteropServices;

namespace NVGRenderer.Rendering.Shaders
{
    internal class UniformManager
    {

        private readonly ulong _fragSize;

        private byte[] _uniforms;
        private int _count;
        private int _capacity;

        public int CurrentsOffset => _count;

        public ReadOnlySpan<byte> Uniforms => _uniforms;

        public UniformManager(ulong fragSize)
        {
            _fragSize = fragSize;
            _uniforms = Array.Empty<byte>();
            _count = 0;
            _capacity = 0;
        }

        private ulong AllocUniforms(int n)
        {
            if (_count + n > _capacity)
            {
                int uniforms = Math.Max(_count + n, 128) + _uniforms.Length / 2;
                Array.Resize(ref _uniforms, uniforms * (int)_fragSize);
                _capacity = uniforms;
            }
            return (ulong)_count * _fragSize;
        }

        public unsafe ulong AddUniform(FragUniforms uniforms)
        {
            ulong ret = AllocUniforms(1);
            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(&uniforms, Marshal.SizeOf(typeof(FragUniforms)));
            Buffer.BlockCopy(bytes.ToArray(), 0, _uniforms, (int)ret, bytes.Length);
            _count += 1;
            return ret;
        }

        public void Clear()
        {
            _count = 0;
        }

    }
}