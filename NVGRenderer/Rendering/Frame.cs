using Engine.Rendering.Veldrid;
using NVGRenderer.Rendering.Shaders;
using Vertex = SilkyNvg.Rendering.Vertex;

namespace NVGRenderer.Rendering
{
    internal class Frame : IDisposable
    {

        private readonly NvgRenderer _renderer;

        public VertexBuffer<Vertex> VertexBuffer { get; }

        public UniformBuffer<VertUniforms> VertexUniformBuffer { get; }

        public UniformBuffer<byte> FragmentUniformBuffer { get; }


        public DescriptorSetManager DescriptorSetManager { get; }

        public unsafe Frame(NvgRenderer renderer)
        {
            _renderer = renderer;

            VertexBuffer = new VertexBuffer<Vertex>(_renderer._device, new Vertex[1]);

            VertexUniformBuffer = new UniformBuffer<VertUniforms>(_renderer._device, 1u);

            FragmentUniformBuffer = new UniformBuffer<byte>(_renderer._device, 1u);

            DescriptorSetManager = new DescriptorSetManager(_renderer);
        }

        public unsafe void Dispose()
        {
            VertexBuffer.Dispose();
            VertexUniformBuffer.Dispose();
            FragmentUniformBuffer.Dispose();
            DescriptorSetManager.Dispose();
        }

    }
}