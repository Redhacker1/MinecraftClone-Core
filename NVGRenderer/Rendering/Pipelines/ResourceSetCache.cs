using NVGRenderer.Rendering.Textures;
using Veldrid;

namespace NVGRenderer.Rendering.Pipelines;

public struct ResourceSetData : IEquatable<ResourceSetData>
{
    public int image;

    public bool Equals(ResourceSetData other)
    {
        return image == other.image;
    }

    public override bool Equals(object obj)
    {
        return obj is ResourceSetData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return image;
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

        if (!_sets.TryGetValue(parameters, out ResourceSet result))
        {
            _ = _nvgFrame.Renderer.TextureManager.FindTexture(parameters.image, out TextureSlot texSlot);

            result = _nvgFrame.Renderer.Device.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(_nvgFrame.Renderer.DescriptorSetLayout,
                    _nvgFrame.VertexUniformBuffer.GetBuffer(),
                    texSlot.TextureSampler,
                    texSlot.Texture._Texture
                ));

            _sets[parameters] = result;
        }
        return result;
        
        
    }
}