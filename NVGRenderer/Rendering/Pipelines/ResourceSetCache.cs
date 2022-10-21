using NVGRenderer.Rendering.Textures;
using Veldrid;

namespace NVGRenderer.Rendering.Pipelines;

public struct ResourceSetData : IEquatable<ResourceSetData>
{
    public int uniformOffset;
    public int image;

    public bool Equals(ResourceSetData other)
    {
        return uniformOffset == other.uniformOffset && image == other.image;
    }

    public override bool Equals(object obj)
    {
        return obj is ResourceSetData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(uniformOffset, image);
    }
}

public class ResourceSetCache
{
    readonly NvgFrame _nvgFrame;
    readonly Dictionary<ResourceSetData, ResourceSet> _sets = new Dictionary<ResourceSetData, ResourceSet>();



    public void Clear()
    {
        foreach (KeyValuePair<ResourceSetData, ResourceSet> set in _sets)
        {
            set.Value.Dispose();
        }
        _sets.Clear();
    }

    public ResourceSetCache(NvgFrame nvgFrame)
    {
        _nvgFrame = nvgFrame;
    }

    public ResourceSet GetResourceSet(ResourceSetData parameters)
    {
        if (_sets.ContainsKey(parameters))
        {
            return _sets[parameters];
        }

        if (_nvgFrame.UniformAllocator.CurrentOffset > _nvgFrame.FragmentUniformBuffer.ByteLength)
        {
            _nvgFrame.FragmentUniformBuffer.Resize(_nvgFrame.UniformAllocator.CurrentOffset / _nvgFrame.UniformAllocator.Alignment);
        }

        uint size = _nvgFrame.FragmentUniformBuffer.ByteLength / _nvgFrame.FragmentUniformBuffer.Length;

        TextureSlot texSlot = _nvgFrame.Renderer.TextureManager.FindTexture(parameters.image, out _);
        _sets[parameters] = _nvgFrame.Renderer.Device.ResourceFactory.CreateResourceSet(
            new ResourceSetDescription(_nvgFrame.Renderer.DescriptorSetLayout,
            _nvgFrame.VertexUniformBuffer.GetBuffer(),
            new DeviceBufferRange(
                _nvgFrame.FragmentUniformBuffer.GetBuffer(), (uint)parameters.uniformOffset, (uint)_nvgFrame.FragmentUniformBuffer.ByteLength - (uint)parameters.uniformOffset),
            texSlot.TextureSampler,
            texSlot._Texture._Texture
        ));

        return _sets[parameters];
    }
}