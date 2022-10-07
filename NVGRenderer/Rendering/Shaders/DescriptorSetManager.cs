using Veldrid;

namespace NVGRenderer.Rendering.Shaders
{
    [Obsolete("Non functional, determine how to replace once finished with other stuff and actually knows what this does and how it is used")]
    internal class DescriptorSetManager : IDisposable
    {

        private readonly ResourceLayout _layout;
        private readonly NvgRenderer _renderer;

        private ResourceSet[] _descriptorSets;
        private int _count;
        private int _capacity;


        public DescriptorSetManager(NvgRenderer renderer)
        {
            _layout = renderer.Shader.DescriptorSetLayout;
            _renderer = renderer;
            _descriptorSets = Array.Empty<ResourceSet>();
            _count = 0;
            _capacity = 0;
        }

        private unsafe void AllocDescriptorSets(uint n)
        {
            if (_count + n > _capacity)
            {
                int cdescriptorSets = Math.Max(_count + (int)n, 16) + (_descriptorSets.Length / 2);
                Array.Resize(ref _descriptorSets, cdescriptorSets);
                _capacity = cdescriptorSets;
            }
        }

        public void Reset(uint requiredDescriptorSetCount)
        {
            if (_capacity < requiredDescriptorSetCount)
            {
                AllocDescriptorSets(requiredDescriptorSetCount);
            }

            _count = 0;
        }

        public ResourceSet GetDescriptorSet()
        {
            return _descriptorSets[_count++];
        }

        public unsafe void Dispose()
        {
        }

    }
}